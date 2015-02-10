using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jerome;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using InputBox;

namespace NetComm
{
    public partial class FMain : Form
    {
        public static int[] lines = { 5, 4, 3, 2, 1, 6 };

        private Dictionary<JeromeConnectionParams, ConnectionState> connections = new Dictionary<JeromeConnectionParams,ConnectionState>();
        private List<ToolStripButton> buttons = new List<ToolStripButton>();
        private List<string> buttonLabels = new List<string>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuControl = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuWatch = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private System.Threading.Timer watchTimer;
        private Color buttonsColor;

        private bool connected
        {
            get
            {
                return connections.Values.ToList().Exists( x => x.active && x.connected );
            }
        }

        private void updateButtonLabel(int no)
        {
            buttons[no].Text = (no + 1).ToString();
            if (!buttonLabels[no].Equals(string.Empty))
                buttons[no].Text += " " + buttonLabels[no];
        }

        public FMain()
        {
            InitializeComponent();
            readConfig();
            foreach ( JeromeConnectionParams c in connections.Keys )
                createConnectionMI( c );
            for (int co = 0; co < lines.Count(); co++)
            {
                ToolStripButton b = new ToolStripButton();
                buttons.Add(b);
                int no = co;
                updateButtonLabel(no);
                b.BackColor = SystemColors.Control;
                b.CheckOnClick = true;
                b.Enabled = false;
                b.CheckedChanged += new EventHandler( delegate( object obj, EventArgs e ) {
                    Parallel.ForEach(connections.Where(x => x.Value.active && x.Value.connected), x =>
                        { x.Value.controller.switchLine(lines[no], b.Checked ? 1 : 0); });
                    if (b.Checked)
                    {
                        buttons.Where(x => x != b).ToList().ForEach(x => x.Checked = false);
                        b.ForeColor = Color.Red;
                    }
                    else
                    {
                        b.ForeColor = buttonsColor;
                    }
                });
                toolStrip.Items.Add(b);
            }
            buttonsColor = buttons[0].ForeColor;
            watchTimer = new System.Threading.Timer( obj => onWatchTimer(), null, 1000, 1000);
        }

        private void onWatchTimer()
        {
            Parallel.ForEach( connections.Where(x => x.Value.watch), x =>
            {
                JeromeControllerState state = x.Key.getState();
                for (int co = 0; co < lines.Count(); co++)
                    x.Value.linesStates[co] = state.linesStates[lines[co] - 1];
            });
            this.Invoke((MethodInvoker)delegate
                { updateButtonsMode(); });
        }

  
        private void createConnectionMI(JeromeConnectionParams c)
        {
            createConnectionMI(c, true);
            createConnectionMI(c, false);
        }


        private void createConnectionMI(JeromeConnectionParams c, bool watch)
        {
            ToolStripMenuItem mi = new ToolStripMenuItem();
            mi.Text = c.name;
            if (watch)
            {
                mi.Visible = !connections[c].active;
                mi.CheckOnClick = true;
                mi.Checked = connections[c].watch;
                mi.Click += delegate(object sender, EventArgs e)
                {
                    connections[c].watch = !connections[c].watch;
                    writeConfig();
                };
                miWatch.DropDownItems.Add(mi);
                menuWatch[c] = mi;
            }
            else
            {
                mi.Visible = !connected;
                mi.Click += delegate(object sender, EventArgs e)
                {
                    if (connections[c].connected)
                        connections[c].controller.disconnect();
                    else
                        if (!connect(c))
                            MessageBox.Show(c.name + ": подключение не удалось!");
                };
                miControl.DropDownItems.Add(mi);
                menuControl[c] = mi;
            }
        }

        private bool connect(JeromeConnectionParams cp)
        {
            connections[cp].controller = JeromeController.create(cp);
            if (connections[cp].controller.connect())
            {
                connections[cp].controller.disconnected += controllerDisconnected;
                connections[cp].watch = false;
                connections[cp].active = true;
                menuWatch[cp].Visible = false;
                menuWatch[cp].Checked = false;
                menuControl[cp].Checked = true;
                updateButtonsMode();
                return true;
            }
            else
                return false;
        }

        private void updateButtonsMode()
        {
            for ( int co = 0; co < lines.Count(); co++ )
                buttons[co].Enabled = connected && 
                    !connections.Values.ToList().Exists(x => x.watch && x.linesStates[co]);
             /*
            if (connections.Values.ToList().Exists(x => x.watch && x.linesStates[no]))
                buttons[no].ForeColor = Color.Red;
            else
                buttons[no].ForeColor = toolStrip.ForeColor;*/
        }

        private void controllerDisconnected(object obj, DisconnectEventArgs e)
        {
            JeromeConnectionParams c = ((JeromeController)obj).connectionParams;
            if (!e.requested)
                MessageBox.Show( c.name + ": cвязь потеряна!");
            connections[c].active = false;
            menuWatch[c].Visible = true;
            menuControl[c].Checked = false;
            updateButtonsMode();
        }

        private void FMain_Load(object sender, EventArgs e)
        {
            Width = 50;
        }

        public void writeConfig()
        {
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\config.xml"))
            {
                AppState s = new AppState();

                s.connections = new JeromeConnectionParams[connections.Count];
                s.watch = new bool[connections.Count];
                int co = 0;
                foreach ( KeyValuePair<JeromeConnectionParams,ConnectionState> x in connections ) {
                    s.connections[co] = x.Key;
                    s.watch[co] = x.Value.watch;
                    co++;
                }

                s.buttonLabels = buttonLabels.ToArray();


                XmlSerializer ser = new XmlSerializer(typeof(AppState));
                ser.Serialize(sw, s);
            }

        }

        private bool readConfig()
        {
            bool result = false;
            if (File.Exists(Application.StartupPath + "\\config.xml"))
            {
                XmlSerializer ser = new XmlSerializer(typeof(AppState));
                using (FileStream fs = File.OpenRead(Application.StartupPath + "\\config.xml"))
                {
                    try
                    {
                        AppState s = (AppState)ser.Deserialize(fs);
                        if ( s.connections != null )                   
                            for ( int co = 0; co < s.connections.Count(); co++ )
                                connections[ s.connections[co] ] = new ConnectionState() { watch = s.watch[co] };
                        else
                            connections = new Dictionary<JeromeConnectionParams,ConnectionState>();
                        if (s.buttonLabels != null)
                            buttonLabels = s.buttonLabels.ToList();
                        else
                            buttonLabels = new List<string>();
                        for ( int co = s.buttonLabels.Count() - 1; co < lines.Count(); co++ )
                            buttonLabels.Add( "" );
                        result = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return result;
        }

        private void miModuleSettings_Click(object sender, EventArgs e)
        {
            (new FModuleSettings()).ShowDialog();
        }

        private void miConnectionsList_Click(object sender, EventArgs e)
        {
            FConnectionsList flist = new FConnectionsList(connections.Keys.ToList());
            flist.connectionCreated += connectionCreated;
            flist.connectionEdited += connectionEdited;
            flist.connectionDeleted += connectionDeleted;
            flist.ShowDialog(this);
        }

        private void connectionCreated(object obj, EventArgs e)
        {
            JeromeConnectionParams c = (JeromeConnectionParams)obj;
            connections[c] = new ConnectionState();
            createConnectionMI(c);
            writeConfig();
        }

        private void connectionEdited(object obj, EventArgs e)
        {
            JeromeConnectionParams c = (JeromeConnectionParams)obj;
            menuControl[c].Text = connections[c].active ? "Отключиться от " + c.name : c.name;
            menuWatch[c].Text = c.name;
            writeConfig();
        }

        private void connectionDeleted(object obj, EventArgs e)
        {
            JeromeConnectionParams c = (JeromeConnectionParams)obj;
            if (connections[c].active)
                MessageBox.Show("Нельзя удалить активное соединение! Изменения не будут сохранены");
            else
            {
                connections.Remove(c);
                menuControl.Remove(c);
                menuWatch.Remove(c);
                writeConfig();
            }
        }


        private void toolStrip_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ToolStripItem i = toolStrip.GetItemAt(new Point(e.X, e.Y));
                if (i != null && i.GetType() == typeof(ToolStripButton) && buttons.Contains(i))
                {
                    int no = buttons.IndexOf((ToolStripButton)i);
                    FInputBox ib = new FInputBox("Переименование кнопки", buttonLabels[no]);
                    ib.StartPosition = FormStartPosition.CenterParent;
                    ib.ShowDialog(this);
                    if (ib.DialogResult == DialogResult.OK)
                    {
                        buttonLabels[no] = ib.value;
                        writeConfig();
                        updateButtonLabel(no);
                    }

                }
            }
        }

    }

    public class ConnectionState
    {
        public bool watch = false;
        public bool active = false;
        public bool[] linesStates;
        public JeromeController controller = null;
        
        public ConnectionState() {
            linesStates =  new bool[FMain.lines.Count()];
            for (int co = 0; co < linesStates.Count(); co++)
                linesStates[co] = false;
                
        }

        public bool connected
        {
            get
            {
                return controller != null && controller.connected;
            }
        }
    }

    public class AppState
    {
        public JeromeConnectionParams[] connections;
        public bool[] watch;
        public string[] buttonLabels;
    }
}
