using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetComm
{
    public partial class FESConnection : Form
    {
        public string host
        {
            get
            {
                return tbAddress.Text;
            }
        }

        public int port
        {
            get
            {
                return Convert.ToInt32(tbPort.Text);
            }
        }

        public FESConnection()
        {
            InitializeComponent();
        }

        public FESConnection( string host, int port)
        {
            InitializeComponent();
            tbAddress.Text = host;
            tbPort.Text = port.ToString();
        }

    
    }
}
