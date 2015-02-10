using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Jerome
{
    public partial class FConnectionsList : Form
    {
        private List<JeromeConnectionParams> connections;
        public event EventHandler connectionEdited;
        public event EventHandler connectionDeleted;
        public event EventHandler connectionCreated;

        public FConnectionsList(List<JeromeConnectionParams> cs)
        {
            InitializeComponent();
            connections = cs;
            fillList();
        }

        private void fillList()
        {
            lbConnections.Items.Clear();
            foreach (JeromeConnectionParams cs in connections)
                lbConnections.Items.Add(cs.name);
            if (connections.Count > 0)
            {
                lbConnections.SelectedIndex = 0;
                bEdit.Enabled = true;
                bDelete.Enabled = true;
            }

        }

        private void bEdit_Click(object sender, EventArgs e)
        {
            if (connections[lbConnections.SelectedIndex].edit())
            {
                int sel = lbConnections.SelectedIndex;
                fillList();
                lbConnections.SelectedIndex = sel;
                if (connectionEdited != null)
                    connectionEdited(connections[sel], null);
            }

        }

        private void bDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить соединение " + lbConnections.Items[lbConnections.SelectedIndex] + "?",
                "Удаление соединения", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                JeromeConnectionParams c = connections[lbConnections.SelectedIndex];
                connections.Remove(c);
                lbConnections.Items.RemoveAt(lbConnections.SelectedIndex);
                if (connectionDeleted != null)
                    connectionDeleted(c, null);
            }
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void bNew_Click(object sender, EventArgs e)
        {
            JeromeConnectionParams nc = new JeromeConnectionParams();
            if (nc.edit())
            {
                connections.Add(nc);
                fillList();
                lbConnections.SelectedIndex = connections.Count - 1;
                if (connectionCreated != null)
                    connectionCreated(nc, null);
            }
        }


    }
}
