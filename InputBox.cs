using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InputBox
{
    public partial class FInputBox : Form
    {
        public string value
        {
            get
            {
                return tbValue.Text;
            }
        }

        public FInputBox( string caption, string value)
        {
            InitializeComponent();
            Text = caption;
            tbValue.Text = value;
            tbValue.SelectAll();
        }
    }
}
