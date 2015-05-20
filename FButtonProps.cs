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
    public partial class FButtonProps : Form
    {
        private int[] _esMHzValues = { };

        public string name
        {
            get
            {
                return tbValue.Text;
            }
        }

        public int[] esMHzValues
        {
            get
            {
                return _esMHzValues; 
            }
        }

        public FButtonProps(string value, string esMHzValues)
        {
            InitializeComponent();
            tbValue.Text = value;
            tbValue.SelectAll();
            tbMhzValues.Text = esMHzValues;
        }

        private void FButtonProps_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !tbMhzValues.Text.Equals(string.Empty))
            {
                try
                {
                    _esMHzValues = tbMhzValues.Text.Split(';').ToList().Select(x => Convert.ToInt32(x)).ToArray();
                }
                catch (FormatException )
                {
                    MessageBox.Show("В поле MHz должны быть целые числа, разделенные точками с запятой!");
                    e.Cancel = true;

                }
            }
        }
    }
}
