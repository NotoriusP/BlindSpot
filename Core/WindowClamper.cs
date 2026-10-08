using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    // The "invisible wall". Scans all top-level windows a few times a second and
    // pushes anything that strays into a dead band back inside the safe zone:
    //   * borderless-fullscreen games  -> resized/moved to exactly the safe zone
    //   * maximised windows            -> normally handled by the work-area lock,
    //                                      but clamped again here as a fallback
    //   * ordinary windows             -> nudged by the smallest amount that clears
    //                                      the band, so we never fight the user
    //
    // A timer (rather than WinEvent hooks) is used on purpose: it survives apps
    // that recreate their window, needs no hook lifetime management, and costs a
    // fraction of a percent of CPU.
    public class WindowClamper
    {
        private readonly AppConfig _config;
        private readonly System.Windows.Forms.Timer _timer;
        // hwnd -> the rect we last asked for, plus when. The clamper must NOT touch a
        // window it just moved: SetWindowPos is applied asynchronously by DWM, so the
        // next tick would see the *old* rect, conclude it failed, and move it again.
        // That feedback loop is the visible "flickering / resizing constantly" bug.
        private readonly Dictionary<IntPtr, MovedRecord> _moved = new Dictionary<IntPtr, MovedRecord>();
        private int _selfProcessId = -1;

        private class MovedRecord
        {
            public Rectangle Target;
            public DateTime When;
        }

        // How long to leave a window alone after we moved it. DWM needs a couple of
        // frames to commit a move, and games re-assert their own size on focus events;
        // a settle window stops us from fighting them.
        private static readonly TimeSpan SettleTime = TimeSpan.FromMilliseconds(1200);

        private const int ViolationTolerance = 10;   // px; ignore shadows / 1px frames
        private const int PositionTolerance = 12;    // px; "close enough" to our target

        public bool Paused { get; set; }

        public WindowClamper(AppConfig config)
        {
            _config = config;
            _timer = new System.Windows.Forms.Timer { Interval = 300 };
            _timer.Tick += OnTick;
            try { _selfProcessId = Process.GetCurrentProcess().Id; } catch { }
        }

        public void Start()
        {
            if (!_timer.Enabled) _timer.Start();
        }

        public void Stop()
        {
            if (_timer.Enabled) _timer.Stop();
        }

        public void ResetCycle()
        {
            // Force the next tick to re-evaluate everything (used after config change).
            _moved.Clear();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Paused || !_config.ProtectionEnabled || !_config.ClampWindows)
                return;
            try
            {
                Win32.EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            }
            catch
            {
            }
            // Forget stale records so the table cannot grow without bound.
            if (_moved.Count > 256)
            {
                var dead = new List<IntPtr>();
                foreach (var kv in _moved)
                {
                    if (DateTime.Now - kv.Value.When > SettleTime) dead.Add(kv.Key);
                }
                foreach (var h in dead) _moved.Remove(h);
            }
        }

        private bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            try
            {
                if (!Win32.IsWindowVisible(hwnd))
                    return true;
                if (Win32.IsIconic(hwnd))
                    return true;

                // Top-level windows only: anything with an owner is a dialog/child.
                if (Win32.GetWindow(hwnd, Win32.GW_OWNER) != IntPtr.Zero)
                    return true;

                if (Win32.GetWindowRect(hwnd, out RECT r) == false)
                    return true;

                // Never touch our own windows (tray, overlays, setup forms).
                Win32.GetWindowThreadProcessId(hwnd, out uint pid);
                if (_selfProcessId > 0 && pid == _selfProcessId)
                    return true;

                var sb = new StringBuilder(256);
                Win32.GetClassName(hwnd, sb, 256);
                string cls = sb.ToString();
                if (cls == "Shell_TrayWnd" || cls == "Progman" || cls == "WorkerW" ||
                    cls == "Button" || cls == "Shell_SecondaryTrayWnd")
                    return true;

                // Let the user exempt a misbehaving game entirely.
                string procName = null;
                try { procName = Process.GetProcessById((int)pid).ProcessName; } catch { }
                if (_config.IsProcessExcluded(procName))
                    return true;

                var screen = Screen.FromHandle(hwnd);
                if (screen == null) return true;

                var zones = ZoneMath.GetDeadZones(_config, screen);
                if (zones.Count == 0)
                    return true;
                var safe = ZoneMath.GetSafeRect(_config, screen);

                // Leave a window alone if we just placed it and it has not had time to
                // settle, or it is already sitting where we put it. This is what stops
                // the tug-of-war where the window flickers between two sizes.
                if (_moved.TryGetValue(hwnd, out var rec))
                {
                    bool stale = DateTime.Now - rec.When > SettleTime;
                    bool atTarget = Math.Abs(r.Left - rec.Target.Left) <= PositionTolerance &&
                                     Math.Abs(r.Top - rec.Target.Top) <= PositionTolerance &&
                                     Math.Abs(r.Right - rec.Target.Right) <= PositionTolerance &&
                                     Math.Abs(r.Bottom - rec.Target.Bottom) <= PositionTolerance;
                    if (!stale || atTarget)
                        return true;
                }

                DebugLog($"probe hwnd={hwnd} cls={cls} proc={procName} rect={r} screen={screen.Bounds} safe={safe} violations={ZoneMath.ViolatesZones(r, zones, ViolationTolerance)}");

                // Already where we want it? Remember so we don't re-clamp.
                if (ZoneMath.IsInsideSafeZone(r, safe))
                {
                    _moved[hwnd] = new MovedRecord { Target = ZoneMath.ScreenRectToNative(r), When = DateTime.Now };
                    return true;
                }

                if (!ZoneMath.ViolatesZones(r, zones, ViolationTolerance))
                    return true;

                long style = Win32.GetWindowStyle(hwnd);
                bool maximised = (style & Win32.WS_MAXIMIZE) != 0;
                bool isPopup = (style & Win32.WS_POPUP) != 0;
                bool hasCaption = (style & Win32.WS_CAPTION) != 0;

                bool coversScreen =
                    r.Left <= screen.Bounds.Left + 4 &&
                    r.Top <= screen.Bounds.Top + 4 &&
                    r.Right >= screen.Bounds.Right - 4 &&
                    r.Bottom >= screen.Bounds.Bottom - 4;

                // A borderless, caption-less window that fills the screen is a
                // borderless-fullscreen game: pin it to the safe zone. It will render
                // at exactly that size, which is what we want.
                bool borderlessGame = isPopup && !hasCaption && (coversScreen || maximised);

                if (borderlessGame)
                {
                    ForceRect(hwnd, safe);
                }
                else if (maximised)
                {
                    // The work-area lock should already cover this; only act if it did not.
                    ForceRect(hwnd, safe);
                }
                else
                {
                    NudgeIntoSafe(hwnd, r, safe);
                }
            }
            catch
            {
                // A window vanishing mid-enumeration is normal; never break the scan.
            }
            return true;
        }

        private void ForceRect(IntPtr hwnd, Rectangle safe)
        {
            // Synchronous (no SWP_ASYNCWINDOWPOS): we need the move committed before
            // the next tick, otherwise we read the stale rect and start fighting.
            Win32.SetWindowPos(hwnd, IntPtr.Zero, safe.Left, safe.Top, safe.Width, safe.Height,
                Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE | Win32.SWP_FRAMECHANGED);
            _moved[hwnd] = new MovedRecord { Target = safe, When = DateTime.Now };
        }

        private static readonly string _logPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BlindSpot", "debug.log");

        private static void DebugLog(string line)
        {
#if DEBUG
            // Verbose window-by-window trace. Release builds skip this entirely so the
            // log file can never grow while the app sits in the tray for days.
            try
            {
                System.IO.File.AppendAllText(_logPath,
                    DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + Environment.NewLine);
            }
            catch { }
#endif
        }

        // Smallest translation (plus a shrink if it simply does not fit) that pulls
        // the window fully inside the safe zone.
        private void NudgeIntoSafe(IntPtr hwnd, RECT r, Rectangle safe)
        {
            int x = r.Left;
            int y = r.Top;
            int w = r.Width;
            int h = r.Height;

            if (w > safe.Width) w = safe.Width;
            if (h > safe.Height) h = safe.Height;

            if (x < safe.Left) x = safe.Left;
            if (y < safe.Top) y = safe.Top;
            if (x + w > safe.Right) x = safe.Right - w;
            if (y + h > safe.Bottom) y = safe.Bottom - h;

            if (x == r.Left && y == r.Top && w == r.Width && h == r.Height)
                return;

            Win32.SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h,
                Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
            _moved[hwnd] = new MovedRecord { Target = new Rectangle(x, y, w, h), When = DateTime.Now };
        }
    }
}
