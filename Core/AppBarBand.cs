using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    // THE DesktopCoral mechanism. Each dead band is registered with the shell as an
    // application desktop toolbar (AppBar). Once registered with ABM_SETPOS, Windows
    // itself reserves that strip of screen:
    //   * maximised windows shrink to the remaining area
    //   * desktop icons rearrange around it (the list view honours the work area)
    //   * the taskbar and most shell furniture avoid it
    // This is what SPI_SETWORKAREA could not do on this build (the shell reverted it
    // immediately). The band window is also the black cover, so one window does both
    // jobs.
    public class AppBarBand : Form
    {
        private bool _registered;
        private readonly AppConfig _config;
        private readonly Rectangle _band;
        private readonly int _edge;

        // The rect we asked the shell to reserve. BandOverlay compares this against
        // the freshly computed zones so it can skip a pointless destroy/re-register
        // cycle (that cycle is what made the desktop flicker every 5 seconds).
        public Rectangle BandRect => _band;
        public int Edge => _edge;
        public bool IsAlive => IsHandleCreated && !IsDisposed;
        public bool IsRegistered => _registered;

        public AppBarBand(AppConfig config, Rectangle band, int edge)
        {
            _config = config;
            _band = band;
            _edge = edge;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = band;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            TopMost = false;
            // A layered + no-activate window never steals focus or appears in Alt+Tab.
            // It stays black and opaque; the alpha is 255 so the dead band is fully
            // covered while letting us paint nothing else.
            AutoScaleMode = AutoScaleMode.None;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                long ex = cp.ExStyle;
                ex |= 0x00000080;    // WS_EX_TOOLWINDOW: no taskbar button
                ex |= 0x08000000;    // WS_EX_NOACTIVATE: never take focus
                cp.ExStyle = (int)ex;
                return cp;
            }
        }

        public void Register()
        {
            if (_registered) return;
            var abd = new Win32.APPBARDATA();
            abd.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            abd.uCallbackMessage = Win32.ABM_CALLBACK;
            Win32.SHAppBarMessage(Win32.ABM_NEW, ref abd);

            // Ask the shell to position us, then propose our own rect.
            abd.uEdge = _edge;
            abd.rc = new RECT(_band.Left, _band.Top, _band.Right, _band.Bottom);
            Win32.SHAppBarMessage(Win32.ABM_QUERYPOS, ref abd);
            Win32.SHAppBarMessage(Win32.ABM_SETPOS, ref abd);
            _registered = true;

            // The shell may have nudged the rect; honour whatever it gave back.
            Bounds = new Rectangle(abd.rc.Left, abd.rc.Top, abd.rc.Width, abd.rc.Height);
            Show();
        }

        public void Unregister()
        {
            if (!_registered) return;
            var abd = new Win32.APPBARDATA();
            abd.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            Win32.SHAppBarMessage(Win32.ABM_REMOVE, ref abd);
            _registered = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Win32.ABM_CALLBACK)
            {
                switch ((int)m.WParam.ToInt64())
                {
                    case Win32.ABN_POSCHANGED:
                        // Something else moved us or the work area changed: re-assert.
                        ReassertPosition();
                        break;
                    case Win32.ABN_FULLSCREENAPP:
                        // A fullscreen app appeared. We deliberately do NOT retreat:
                        // the whole point is that games avoid our band. The resolution
                        // lock is the tool for exclusive-fullscreen, not retreating.
                        break;
                }
            }
            const int WM_MOUSEACTIVATE = 0x0021;
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = new IntPtr(0x0003);   // MA_NOACTIVATE
                return;
            }
            base.WndProc(ref m);
        }

        private void ReassertPosition()
        {
            var abd = new Win32.APPBARDATA();
            abd.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            abd.uEdge = _edge;
            abd.rc = new RECT(_band.Left, _band.Top, _band.Right, _band.Bottom);
            Win32.SHAppBarMessage(Win32.ABM_QUERYPOS, ref abd);
            Win32.SHAppBarMessage(Win32.ABM_SETPOS, ref abd);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Unregister();
            base.OnHandleDestroyed(e);
        }
    }
}
