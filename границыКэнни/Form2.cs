using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace границыКэнни
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_Closing(object sender, FormClosingEventArgs e)
        {
            Hide();
            Form1 formInfo = new Form1();
            formInfo.Show();
        }
    }
}
