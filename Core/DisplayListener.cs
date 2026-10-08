using System;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    // A message-only window that listens for WM_DISPLAYCHANGE (a game just switched
    // the display into exclusive-fullscreen, or the resolution was reset) and owns
    // the global pause hotkey.
    public class DisplayListener : NativeWindow
    {
        public const int PauseHotKeyId = 0x5001;

        public event EventHandler DisplayChanged;

        private bool _hotkeyRegistered;

        public DisplayListener()
        {
            var cp = new CreateParams();
            cp.Caption = "BlindSpotListener";
            cp.Parent = new IntPtr(-3);   // HWND_MESSAGE: message-only window
            CreateHandle(cp);
        }

        public bool RegisterPauseHotkey()
        {
            if (_hotkeyRegistered) return true;
            // Ctrl + Alt + P
            _hotkeyRegistered = Win32.RegisterHotKey(Handle, PauseHotKeyId,
                Win32.MOD_CONTROL | Win32.MOD_ALT | Win32.MOD_NOREPEAT, (int)Keys.P);
            return _hotkeyRegistered;
        }

        public void UnregisterPauseHotkey()
        {
            if (_hotkeyRegistered)
            {
                Win32.UnregisterHotKey(Handle, PauseHotKeyId);
                _hotkeyRegistered = false;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == Win32.WM_DISPLAYCHANGE)
            {
                DisplayChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (m.Msg == WM_HOTKEY && m.WParam.ToInt64() == PauseHotKeyId)
            {
                PauseHotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public event EventHandler PauseHotkeyPressed;

        public override void DestroyHandle()
        {
            UnregisterPauseHotkey();
            base.DestroyHandle();
        }
    }
}
