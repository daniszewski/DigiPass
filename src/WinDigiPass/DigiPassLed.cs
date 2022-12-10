using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardCmd
{
    internal class DigiPassLed
    {
        const int DELAY = 20;
        public event EventHandler OnTransferProgress;

        byte[] buffer = new byte[32];
        Thread callingThread;

        public DigiPassLed(string password)
        {
            if (password.Length > 31) throw new ArgumentException("Password must be maximum 31 chars length");

            Array.Clear(buffer, 0, buffer.Length);
            var data = Encoding.ASCII.GetBytes(password);
            Array.Copy(data, buffer, data.Length);
            buffer[buffer.Length - 1] = GetCRC();
            CRC = buffer[buffer.Length - 1].ToString();
        }

        public string CRC { get; internal set; }
        public int Progress { get; internal set; }

        private byte GetCRC()
        {
            return (byte)(new BitArray(buffer).Cast<bool>().Count(l => l) & 127);
        }

        public void StartTransfer()
        {
            int i = 0;
            callingThread = Thread.CurrentThread;
            ReportProgress(i);
            var bits = new BitArray(buffer);
            new Thread(new ThreadStart(() =>
            {
                KeyboardLeds.CL = true;
                Thread.Sleep(DELAY);
                foreach (bool bit in bits)
                {
                    if (bit)
                        KeyboardLeds.SL = !KeyboardLeds.SL;
                    else
                        KeyboardLeds.NL = !KeyboardLeds.NL;
                    Thread.Sleep(DELAY);
                    ReportProgress(++i);
                }
                KeyboardLeds.CL = false;
                Thread.Sleep(50);
                KeyboardLeds.NL = false;
                KeyboardLeds.SL = false;
            })).Start();
        }

        private void ReportProgress(int progress)
        {
            Progress = progress;
            OnTransferProgress?.Invoke(this, EventArgs.Empty);
        }
    }
}
