using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardCmd
{
    public static class KeyboardLeds
    {
        [DllImport("user32.dll")]
        static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        static void PressKeyboardButton(Keys keyCode)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;

            keybd_event((byte)keyCode, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event((byte)keyCode, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static bool NL
        {
            get { return GetKeyState((int)Keys.NumLock) != 0; }
            set { if (NL != value) { PressKeyboardButton(Keys.NumLock); } }
        }

        public static bool SL
        {
            get { return GetKeyState((int)Keys.Scroll) != 0; }
            set { if (SL != value) { PressKeyboardButton(Keys.Scroll); } }
        }

        public static bool CL
        {
            get { return GetKeyState((int)Keys.CapsLock) != 0; }
            set { if (CL != value) { PressKeyboardButton(Keys.CapsLock); } }
        }
    }
}
