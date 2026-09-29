using HFromUI.HSocket.HTcpClient.ShengGuang;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUITestB
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void hPushButton1_Click(object sender, EventArgs e)
        {
            ShengGuangData s = new ShengGuangData();
        }

        private void hHopper1_ValueChanged(object sender, EventArgs e)
        {

        }
        private ShengGuangDataIPC shengGuangData =new ShengGuangDataIPC();
        private void hPushButton1_Click_1(object sender, EventArgs e)
        {
            shengGuangData.Run();
            timer1.Start();
        }

        private void hAffixTextBox1_TextChanged(object sender, EventArgs e)
        {
            shengGuangData.PipeName= hAffixTextBox1.Text;
        }

        private void hAffixTextBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
             //   shengGuangData.Port = Convert.ToInt32(hAffixTextBox2.Text);

            }
            catch { }
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            shengGuangData.Add(richTextBox1.Text);
        }

        private void hPushButton2_Click(object sender, EventArgs e)
        {
            shengGuangData.Stop();
            timer1.Stop();
        }
    }
}
