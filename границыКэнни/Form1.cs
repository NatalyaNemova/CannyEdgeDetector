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
    public partial class Form1 : Form
    {
       

        public Form1()
        {
            InitializeComponent();
            
        }
        Handler CannyData;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog { RestoreDirectory = true, FilterIndex = 1 };
            OpenFileDialog.Filter = "Img File(*.BMP;*.JPG;*.PNG)|)*.BMP;*.JPG;*.PNG|All files(*.*)|*.*";
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    pictureBox1.Image = Bitmap.FromFile(OpenFileDialog.FileName);

                }
                catch (ApplicationException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            float TH, TL, Sigma;
            int MaskSize;

           
            //TxtTH - верхний порог textBox1
            //TxtTL - нижний порог
            TH = (float)Convert.ToDouble(textBox1.Text);
            TL = (float)Convert.ToDouble(textBox2.Text);

            MaskSize = 5;
            Sigma = (float)Convert.ToDouble(textBox4.Text);
            
            CannyData = new Handler((Bitmap)pictureBox1.Image, TH, TL, MaskSize, Sigma);

            pictureBox2.Image = CannyData.DisplayImage(CannyData.FilteredImage);

            pictureBox3.Image = CannyData.DisplayImage(CannyData.NonMax);

            pictureBox4.Image = CannyData.DisplayImage(CannyData.GNH);

            pictureBox5.Image = CannyData.DisplayImage(CannyData.GNL);

            pictureBox6.Image = CannyData.DisplayImage(CannyData.EdgeMap);

           
            
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap bmpSave = (Bitmap)pictureBox6.Image;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "jpg";
            sfd.Filter = "Image files (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)

                bmpSave.Save(sfd.FileName, ImageFormat.Jpeg);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            Hide();
            Form2 formInfo = new Form2();
            formInfo.Show();
        }
    }
}
