using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace KeyboardCmd
{
    public class DigiPassLed
    {
        public event EventHandler OnTransferProgress;

        readonly byte[] buffer = new byte[32];

        public DigiPassLed(string password)
        {
            if (password.Length > 31) throw new ArgumentException("Password must be maximum 31 chars length");

            Array.Clear(buffer, 0, buffer.Length);
            var data = Encoding.ASCII.GetBytes(password);
            Array.Copy(data, buffer, data.Length);
            buffer[buffer.Length - 1] = GetCRC();
            CRC = buffer[buffer.Length - 1].ToString();
        }

        public int BitDelay { get; internal set; } = 20;
        public string CRC { get; internal set; }
        public int Progress { get; internal set; }

        private byte GetCRC()
        {
            return (byte)(new BitArray(buffer).Cast<bool>().Count(l => l) & 127);
        }

        public BitArray GetBufferBitArray()
        {
            return new BitArray(buffer);
        }

        public void Transfer()
        {
            Progress = 0;
            OnTransferProgress?.Invoke(this, EventArgs.Empty);
            var bits = new BitArray(buffer);
            Thread.Sleep(BitDelay);
            KeyboardLeds.CL = true;
            Thread.Sleep(BitDelay * 2);
            foreach (bool bit in bits)
            {
                if (bit) KeyboardLeds.SL = !KeyboardLeds.SL;
                else KeyboardLeds.NL = !KeyboardLeds.NL;
                Thread.Sleep(BitDelay);
                Progress++;
                OnTransferProgress?.Invoke(this, EventArgs.Empty);
            }
            KeyboardLeds.CL = false;
            Thread.Sleep(BitDelay * 2);
            KeyboardLeds.NL = false;
            KeyboardLeds.SL = false;
        }
    }
}
