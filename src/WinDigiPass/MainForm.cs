using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardCmd
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            checkBox1.Checked = KeyboardLeds.CL;
            checkBox2.Checked = KeyboardLeds.SL;
            checkBox3.Checked = KeyboardLeds.NL;
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            KeyboardLeds.CL = checkBox1.Checked;
            textBox1.Focus();
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            KeyboardLeds.SL = checkBox2.Checked;
            textBox1.Focus();
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            KeyboardLeds.NL = checkBox3.Checked;
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Focus();

            var digiPass = new DigiPassLed(textBox2.Text);
            label1.Text = $"CRC: {digiPass.CRC}";
            digiPass.OnTransferProgress += DigiPass_OnTransferProgress;
            digiPass.StartTransfer();
        }

        private void DigiPass_OnTransferProgress(object sender, EventArgs e)
        {
            var progress = (sender as DigiPassLed).Progress;
            progressBar1.Value = progress>progressBar1.Maximum ? progressBar1.Maximum : progress;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = checkBox4.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = GeneratePassword();
        }

        private static string GeneratePassword()
        {
            var chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var len = 17;
            var sb = new StringBuilder();
            var rnd = new Random();
            while (len-- > 0)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }
    }
}
