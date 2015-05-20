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
using ExpertSync;
using System.Net;

namespace NetComm
{
    public partial class FMain : Form
    {
        public static int[] lines = { 5, 4, 3, 2, 1, 6 };
        //public static int buttonsQty = 6;

        private Dictionary<JeromeConnectionParams, JeromeConnectionState> connections = new Dictionary<JeromeConnectionParams,JeromeConnectionState>();
        private List<CheckBox> buttons = new List<CheckBox>();
        private List<string> buttonLabels = new List<string>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuControl = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuWatch = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private System.Threading.Timer watchTimer;
        private Color buttonsColor;
        private Dictionary<int, int> esBindings = new Dictionary< int, int> ();
        private ExpertSyncConnector esConnector;
        private IPEndPoint esEndPoint;

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
            Width = 200;
            readConfig();
            for (int co = buttonLabels.Count(); co < lines.Count(); co++)
                buttonLabels.Add("");
            miRelaySettings.Enabled = connections.Count > 0;
            foreach ( JeromeConnectionParams c in connections.Keys )
                createConnectionMI( c );
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
            for ( int co = 0; co < buttons.Count(); co++ )
                buttons[co].Enabled = connected && 
                    !connections.Values.ToList().Exists(x => x.watch && x.linesStates[co]);
        }

        private void controllerDisconnected(object obj, Jerome.DisconnectEventArgs e)
        {
            JeromeConnectionParams c = ((JeromeController)obj).connectionParams;
            if (!e.requested)
                MessageBox.Show( c.name + ": cвязь потеряна!", "NetCommAnt" );
            connections[c].active = false;
            this.Invoke((MethodInvoker)delegate
            {
                menuWatch[c].Visible = true;
                menuControl[c].Checked = false;
                updateButtonsMode();
            });
        }

        private void FMain_Load(object sender, EventArgs e)
        {
            Width = 100;
            for (int co = 0; co < lines.Count(); co++)
            {
                CheckBox b = new CheckBox();
                b.Height = 25;
                b.Width = Width - 7;
                b.Top = 25 + (b.Height + 2) * co;
                b.Left = 1;
                b.TextAlign = ContentAlignment.MiddleCenter;
                buttons.Add(b);
                int no = co;
                updateButtonLabel(no);
                b.BackColor = SystemColors.Control;
                b.Appearance = Appearance.Button;
                b.Enabled = false;
                b.CheckedChanged += new EventHandler(delegate(object obj, EventArgs ea)
                {
                    if (b.Checked)
                    {
                        buttons.Where(x => x != b).ToList().ForEach(x => x.Checked = false);
                        b.ForeColor = Color.Red;
                    }
                    else
                    {
                        b.ForeColor = buttonsColor;
                    }
                    System.Diagnostics.Debug.WriteLine(b.Text + (b.Checked ? " on" : " off"));
                    Parallel.ForEach(connections.Where(x => x.Value.active && x.Value.connected), x =>
                    { x.Value.controller.switchLine(lines[x.Value.lines[no] - 1], b.Checked ? 1 : 0); });
                });
                b.MouseDown += form_MouseClick;
                Controls.Add(b);
            }
            buttonsColor = buttons[0].ForeColor;
            watchTimer = new System.Threading.Timer(obj => onWatchTimer(), null, 1000, 1000);

        }

        public void writeConfig()
        {
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\config.xml"))
            {
                AppState s = new AppState();

                s.connections = new JeromeConnectionParams[connections.Count];
                s.states = new JeromeConnectionState[connections.Count];
                int co = 0;
                foreach (KeyValuePair<JeromeConnectionParams, JeromeConnectionState> x in connections)
                {
                    s.connections[co] = x.Key;
                    s.states[co] = x.Value;
                    co++;
                }

                s.buttonLabels = buttonLabels.ToArray();

                s.esMhzValues = new int[ esBindings.Count ];
                s.esButtons = new int[esBindings.Count];
                co = 0;
                foreach ( KeyValuePair<int,int> x in esBindings ) {
                    s.esMhzValues[co] = x.Key;
                    s.esButtons[co] = x.Value;
                    co++;
                }

                if (esEndPoint != null)
                {
                    s.esHost = esEndPoint.Address.ToString();
                    s.esPort = esEndPoint.Port;
                }

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
                        if (s.connections != null)
                            for (int co = 0; co < s.connections.Count(); co++)
                            {
                                s.states[co].active = false;
                                connections[s.connections[co]] = s.states[co];
                            }
                        if (s.buttonLabels != null) 
                            buttonLabels = s.buttonLabels.ToList();
                        if ( s.esMhzValues != null )
                            for (int co = 0; co < s.esButtons.Count(); co++)
                                esBindings[s.esMhzValues[co]] = s.esButtons[co];
                        IPAddress hostIP;
                        if (IPAddress.TryParse(s.esHost, out hostIP))
                            esEndPoint = new IPEndPoint(hostIP, s.esPort);
                        result = true;
                    }
                    catch (Exception)
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
            connections[c] = new JeromeConnectionState();
            createConnectionMI(c);
            miRelaySettings.Enabled = true;
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
                miControl.DropDownItems.Remove(menuControl[c]);
                menuControl.Remove(c);
                miWatch.DropDownItems.Remove(menuWatch[c]);
                menuWatch.Remove(c);
                miRelaySettings.Enabled = connections.Count > 0;
                writeConfig();
            }
        }


        private void form_MouseClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse click");
            if (e.Button == MouseButtons.Right)
            {
                CheckBox b = null;
                if (sender.GetType() == typeof(CheckBox))
                    b = (CheckBox)sender;
                else
                {
                    Control i = GetChildAtPoint(new Point(e.X, e.Y));
                    if (i != null && i.GetType() == typeof(CheckBox))
                        b = (CheckBox)i;
                }
                if ( b != null && buttons.Contains(b))
                {
                    int no = buttons.IndexOf(b);
                    string bindStr = string.Join("; ", esBindings.Where(x => x.Value == no).Select(x => x.Key).ToArray());
                    FButtonProps ib = new FButtonProps(buttonLabels[no], bindStr);
                    ib.StartPosition = FormStartPosition.CenterParent;
                    ib.ShowDialog(this);
                    if (ib.DialogResult == DialogResult.OK)
                    {
                        buttonLabels[no] = ib.name;
                        b.Text = ib.name;
                        foreach (KeyValuePair<int, int> x in esBindings.Where(x => x.Value == no).ToList())
                            esBindings.Remove(x.Key);
                        foreach (int mhz in ib.esMHzValues)
                            esBindings[mhz] = no;
                        writeConfig();
                        updateButtonLabel(no);
                    }
                }
            }
        }

        private void miRelaySettings_Click(object sender, EventArgs e)
        {
            FRelaySettings frs = new FRelaySettings(connections, buttonLabels);
            frs.ShowDialog();
            if (frs.DialogResult == DialogResult.OK)
            {
                for (int co = 0; co < lines.Count(); co++)
                    connections[frs.connection].lines[co] = frs.cbLines[co].SelectedIndex + 1;
                writeConfig();
            }
        }

        private void miExpertSync_Click(object sender, EventArgs e)
        {
            if (miExpertSync.Checked)
            {
                FESConnection fes;
                if (esEndPoint != null)
                    fes = new FESConnection(esEndPoint.Address.ToString(), esEndPoint.Port);
                else
                    fes = new FESConnection();
                fes.ShowDialog();
                if (fes.DialogResult == DialogResult.OK)
                {
                    esConnector = ExpertSyncConnector.create(fes.host, fes.port);
                    esEndPoint = new IPEndPoint(IPAddress.Parse(fes.host), fes.port);
                    esConnector.disconnected += esDisconnected;
                    esConnector.onMessage += esMessage;
                    writeConfig();
                    miExpertSync.Checked = esConnector.connect();
                }
            }
            else
            {
                esConnector.disconnect();
                miExpertSync.Checked = false;
            }
        }

        private void esDisconnected(object sender, ExpertSync.DisconnectEventArgs e)
        {
            if (!e.requested)
                MessageBox.Show("Соединение с ExpertSync потеряно!");
            miExpertSync.Checked = false;
        }

        private void esMessage(object sender, MessageEventArgs e)
        {
            int mhz = ((int)e.vfoa) / 1000000;
            if ( esBindings.ContainsKey( mhz ) ) 
                this.Invoke( (MethodInvoker) delegate {
                    buttons[esBindings[mhz]].Checked = true;
                });
        }



    }

    public class JeromeConnectionState
    {
        public bool watch = false;
        public bool active = false;
        public bool[] linesStates;
        public JeromeController controller = null;
        public int[] lines;
        
        public JeromeConnectionState() {
            linesStates =  new bool[FMain.lines.Count()];
            lines = new int[FMain.lines.Count()];
            for (int co = 0; co < linesStates.Count(); co++)
            {
                linesStates[co] = false;
                lines[co] = co + 1;
            }
                
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
        public JeromeConnectionState[] states;
        public string[] buttonLabels;
        public int[] esMhzValues;
        public int[] esButtons;
        public string esHost;
        public int esPort;
    }
}
