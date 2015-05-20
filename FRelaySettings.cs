using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jerome;

namespace NetComm
{
    public partial class FRelaySettings : Form
    {
        private JeromeConnectionParams[] _lConnections;
        private Dictionary<JeromeConnectionParams, JeromeConnectionState> _connections;
        public ComboBox[] cbLines;
        public JeromeConnectionParams connection;

        public FRelaySettings(Dictionary<JeromeConnectionParams, JeromeConnectionState> connections, List<string> buttonLabels)
        {
            InitializeComponent();
            _connections = connections;
            _lConnections = _connections.Keys.ToArray();
            cbConnection.Items.AddRange(_lConnections.Select( x => x.name ).ToArray());
            cbLines = new ComboBox[buttonLabels.Count()];
            for ( int co = 0; co < buttonLabels.Count(); co++ ) {
                ComboBox cb = new ComboBox();
                for ( int co1 = 0; co1 < FMain.lines.Count(); co1++ )
                    cb.Items.Add( ( co1 + 1 ).ToString() );
                cb.Top = 50 + 35 * co;
                cb.Left = 134;
                Label l = new Label();
                l.Text = buttonLabels[co].Equals( String.Empty ) ? ( co+1).ToString() : buttonLabels[co];
                l.Top = cb.Top;
                l.Left = 15;
                this.Controls.Add(cb);
                this.Controls.Add(l);
                cbLines[co] = cb;
            }
            cbConnection.SelectedIndex = 0;
        }

        private void cbConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            connection = _lConnections[cbConnection.SelectedIndex];
            for (int co = 0; co < cbLines.Count(); co++)
                if (_connections[connection].lines.Count() > co)
                    cbLines[co].SelectedIndex = _connections[connection].lines[co] - 1;
                else
                    cbLines[co].SelectedIndex = -1;
        }

        
    }
}
