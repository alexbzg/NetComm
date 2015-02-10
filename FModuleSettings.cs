using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Jerome
{
    public partial class FModuleSettings : Form
    {
        TcpClient socket;
        public FModuleSettings()
        {
            InitializeComponent();
        }

        private void socketWrite(string cmd)
        {
            byte[] byteCmd = System.Text.ASCIIEncoding.ASCII.GetBytes("$KE," + cmd + "\r\n");
            socket.GetStream().Write(byteCmd, 0, byteCmd.Length);

        }

        private string socketRead()
        {
            string result = "";
            while ( !result.Contains("\n") )
            {
                while (socket.Available > 0)
                {
                    result += (char)socket.GetStream().ReadByte();
                }
            }
            return result;
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            if ( ( ( !tbNewIP.Text.Equals( tbHost.Text ) && ( tbNewIP.Text.Length > 0 ) ) ||
                ( !tbNewPassword.Text.Equals( tbPassword.Text ) && ( tbNewPassword.Text.Length > 0 ) ) ) &&
                    ( tbHost.Text.Length > 0 ) && ( tbPort.Text.Length > 0 ) && ( tbPassword.Text.Length > 0 ) ) {

            socket = new TcpClient(tbHost.Text, Convert.ToInt32(tbPort.Text));

            if (socket.Connected)
            {
                socketWrite( "PSW,SET," + tbPassword.Text );
                if (socketRead().Contains("OK"))
                {
                    if ((!tbNewIP.Text.Equals(tbHost.Text) && (tbNewIP.Text.Length > 0)))
                    {
                        socketWrite( "IP,SET," + tbNewIP.Text );
                        if (socketRead().Contains("OK"))
                        {
                            socketWrite("MAC,SET,0.4.163.0." + tbNewIP.Text.Split(".".ToCharArray(), 3)[2]);
                            if (!socketRead().Contains("OK"))
                            {
                                MessageBox.Show("Error setting IP address!");
                                socket.Close();
                                return;
                            }
                            socketWrite("RST");
                        }
                        else
                        {
                            MessageBox.Show("Error setting IP address!");
                            socket.Close();
                            return;
                        }
                    }
                    if (!tbNewPassword.Text.Equals(tbPassword.Text) && (tbNewPassword.Text.Length > 0))
                    {
                        socketWrite("PSW,NEW," + tbPassword.Text + "," + tbNewPassword.Text);
                        if (!socketRead().Contains("OK"))
                        {
                            MessageBox.Show("Error changing Password!");
                            socket.Close();
                            return;
                        }
                    }
                    socketWrite("RST");
                }
                else
                {
                    MessageBox.Show("Wrong password!");
                    socket.Close();
                    return;
                }
                this.DialogResult = DialogResult.OK;
                MessageBox.Show("Module settings changed succefully");
                socket.Close();
                this.Close();
            }
        }

        }
    }
}
