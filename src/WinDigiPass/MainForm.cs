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
        DigiPassLed transfer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            numericUpDown1.Enabled = false;
            textBox2.Enabled = false;
            transfer = new DigiPassLed(textBox2.Text);
            transfer.BitDelay = (int)numericUpDown1.Value;
            //DEBUG ONLY:
            //textBox1.AppendText(Environment.NewLine + "CRC: " + transfer.CRC + Environment.NewLine);
            //textBox1.AppendText(string.Join("", transfer.GetBufferBitArray().OfType<bool>().Select(x => x ? "#" : "-")) + Environment.NewLine);
            transfer.OnTransferProgress += DigiPass_OnTransferProgress;
            textBox1.Focus();
            textBox1.SelectionStart = textBox1.Text.Length;
            new Thread(new ThreadStart(transfer.Transfer)).Start();
        }

        private void DigiPass_OnTransferProgress(object sender, EventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action<object, EventArgs>(DigiPass_OnTransferProgress), sender, e); }
            else
            {
                var progress = (sender as DigiPassLed).Progress;
                progressBar1.Value = progress > progressBar1.Maximum ? progressBar1.Maximum : progress;
                if (progress >= progressBar1.Maximum)
                {
                    button1.Enabled = true;
                    button2.Enabled = true;
                    numericUpDown1.Enabled = true;
                    textBox2.Enabled = true;
                }
            }
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
