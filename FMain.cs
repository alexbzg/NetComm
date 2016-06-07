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
        public static int[] lines = { 5, 4, 3, 2, 1, 6, 7 };
        //public static int buttonsQty = 6;

        private Dictionary<JeromeConnectionParams, ConnectionFormState> connections = new Dictionary<JeromeConnectionParams,ConnectionFormState>();
        private List<CheckBox> buttons = new List<CheckBox>();
        private List<string> buttonLabels = new List<string>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuControl = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private Dictionary<JeromeConnectionParams, ToolStripMenuItem> menuWatch = new Dictionary<JeromeConnectionParams, ToolStripMenuItem>();
        private System.Threading.Timer watchTimer;
        private Color buttonsColor;
        private Dictionary<int, int> esBindings = new Dictionary< int, int> ();
        private ExpertSyncConnector esConnector;
        private IPEndPoint esEndPoint;
        private AppState appState;
        private bool loaded = false;
        private JeromeConnectionParams connectionFromArgs = null;
        private bool formSPModified = false;

        private bool connected
        {
            get
            {
                return connections.Values.ToList().Exists( x => x.active && x.connected );
            }
        }

        private void updateButtonLabel(int no)
        {
            if (buttonLabels[no].Equals(string.Empty))
                buttons[no].Text = (no + 1).ToString();
            else
                buttons[no].Text = buttonLabels[no];
        }

        public FMain( string[] args )
        {
            InitializeComponent();
            Width = 200;
            readConfig();
            if (args.Count() > 0)
                connectionFromArgs = connections.Keys.ToList().Find(item => item.name.Equals(args[0]));
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
                this.Text = cp.name;
                updateButtonsMode();
                string linesState = connections[cp].controller.readlines();
                for (var c = 0; c < lines.Count(); c++)
                {
                    connections[cp].controller.setLineMode(lines[c], 0);
                    buttons[c].Checked = linesState[lines[c] - 1] == '1';
                }
                if ( !formSPModified && !connections[cp].formLocation.IsEmpty )
                    this.DesktopBounds =
                            new Rectangle(connections[cp].formLocation, connections[cp].formSize);
                writeConfig();

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
                MessageBox.Show( c.name + ": cвязь потеряна!", "Коммутатор антенн" );
            connections[c].active = false;
            this.Invoke((MethodInvoker)delegate
            {
                menuWatch[c].Visible = true;
                menuControl[c].Checked = false;
                updateButtonsMode();
                this.Text = "Ant Comm";
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
                b.Anchor = AnchorStyles.Right | AnchorStyles.Left;
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
            if (appState != null )
            {
                if (connectionFromArgs != null)
                    connect(connectionFromArgs);
                else if (appState.lastConnection > -1 && connections.ContainsKey( appState.connections[appState.lastConnection] ) )
                    connect(appState.connections[appState.lastConnection]);

            }
            loaded = true;
        }

        
        public void writeConfig()
        {
            if (!loaded)
                return;
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\config.xml"))
            {
                AppState s = new AppState();
                s.lastConnection = -1;

                s.connections = new JeromeConnectionParams[connections.Count];
                s.states = new ConnectionFormState[connections.Count];
                int co = 0;
                foreach (KeyValuePair<JeromeConnectionParams, ConnectionFormState> x in connections)
                {
                    if (x.Value.active)
                    {
                        s.lastConnection = co;
                        System.Drawing.Rectangle bounds = this.WindowState != FormWindowState.Normal ? this.RestoreBounds : this.DesktopBounds;
                        x.Value.formLocation = bounds.Location;
                        x.Value.formSize = bounds.Size;
                        formSPModified = false;
                    }
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
                        appState = (AppState)ser.Deserialize(fs);
                        if (appState.connections != null)
                            if ( appState.states == null || appState.states.Count() == 0 )
                                appState.states = new ConnectionFormState[appState.connections.Count()];
                            for (int co = 0; co < appState.connections.Count(); co++)
                            {
                                if (appState.states[co] == null)
                                    appState.states[co] = new ConnectionFormState();
                                appState.states[co].active = false;
                                connections[appState.connections[co]] = appState.states[co];
                            }
                        if (appState.buttonLabels != null) 
                            buttonLabels = appState.buttonLabels.ToList();
                        if ( appState.esMhzValues != null )
                            for (int co = 0; co < appState.esButtons.Count(); co++)
                                esBindings[appState.esMhzValues[co]] = appState.esButtons[co];
                        IPAddress hostIP;
                        if (IPAddress.TryParse(appState.esHost, out hostIP))
                            esEndPoint = new IPEndPoint(hostIP, appState.esPort);
                        result = true;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
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
            connections[c] = new ConnectionFormState();
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
            //System.Diagnostics.Debug.WriteLine("Mouse click");
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


        private void FMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9 && e.KeyCode - Keys.NumPad0 - 1 < buttons.Count)
            {
                CheckBox b = buttons[e.KeyCode - Keys.NumPad0 - 1];
                if (b.Enabled)
                    b.Checked = !b.Checked;
               /* else
                    MessageBox.Show((e.KeyCode - Keys.NumPad0 - 1).ToString());*/
            }
        }

        private void FMain_ResizeEnd(object sender, EventArgs e)
        {
            int bHeight = ( this.ClientSize.Height - 50 ) / lines.Count() - 2;
            for (int co = 0; co < buttons.Count(); co++)
            {
                CheckBox b = buttons[co];
                b.Height = bHeight;
                b.Top = 25 + (bHeight + 2) * co;
            }
            if (!connections.Values.ToList().Exists(item => item.active))
                formSPModified = true;
            writeConfig();
        }

        private void FMain_LocationChanged(object sender, EventArgs e)
        {
            if (!connections.Values.ToList().Exists(item => item.active))
                formSPModified = true;
            writeConfig();
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

    public class ConnectionFormState : JeromeConnectionState
    {
        public System.Drawing.Point formLocation;
        public System.Drawing.Size formSize;
    }

    public class AppState
    {
        public JeromeConnectionParams[] connections;
        public ConnectionFormState[] states;
        public string[] buttonLabels;
        public int[] esMhzValues;
        public int[] esButtons;
        public string esHost;
        public int esPort;
        public int lastConnection;
    }
}
