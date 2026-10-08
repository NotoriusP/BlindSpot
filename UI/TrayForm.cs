using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlindSpot.Core;
using BlindSpot.Native;

namespace BlindSpot.UI
{
    // The tray shell. Owns every subsystem, wires the menu to them, and keeps the
    // icon text in sync with the current state.
    public class TrayForm : Form
    {
        private readonly AppConfig _config;
        private readonly WindowClamper _clamper;
        private readonly BandOverlay _overlay;
        private readonly ResolutionManager _resolution;
        private readonly DisplayListener _listener;
        private readonly System.Windows.Forms.Timer _refreshTimer;

        private NotifyIcon _tray;
        private ContextMenuStrip _menu;
        private ToolStripMenuItem _miEnable, _miPause, _miLockRes, _miRestoreRes, _miExclude, _miLang;
        private Label _dummy;

        private DateTime _lastRelock = DateTime.MinValue;

        public TrayForm()
        {
            _config = AppConfig.Load();
            Lang.Current = (_config.Language == Lang.En) ? Lang.En : Lang.Fa;
            _clamper = new WindowClamper(_config);
            _overlay = new BandOverlay(_config);
            _resolution = new ResolutionManager();
            _listener = new DisplayListener();
            _listener.DisplayChanged += OnDisplayChanged;
            _listener.PauseHotkeyPressed += (s, e) => TogglePause();

            // Lightweight refresh loop: re-asserts the AppBar bands. The shell can
            // drop an appbar registration after a display-mode flip (a game going
            // exclusive), so we re-register periodically rather than trusting it.
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) =>
            {
                if (_config.ProtectionEnabled)
                    _overlay.Apply();
            };

            BuildTray();
            ApplyProtection();
            Visible = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Opacity = 0;
            WindowState = FormWindowState.Minimized;
            Load += (s, e) =>
            {
                Visible = false;
                _listener.RegisterPauseHotkey();
                _refreshTimer.Start();
            };
        }

        private void BuildTray()
        {
            _dummy = new Label();
            Controls.Add(_dummy);

            _tray = new NotifyIcon
            {
                Icon = MakeIcon(),
                Visible = true,
                Text = "BlindSpot"
            };

            _menu = new ContextMenuStrip { RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No };
            var miSettings = new ToolStripMenuItem(Lang.T("m_settings"), null, (s, e) => OpenSettings());
            var miDraw = new ToolStripMenuItem(Lang.T("m_draw"), null, (s, e) => OpenDrawer());
            _miEnable = new ToolStripMenuItem(Lang.T("m_enable")) { Checked = _config.ProtectionEnabled };
            _miEnable.Click += (s, e) =>
            {
                _config.ProtectionEnabled = !_config.ProtectionEnabled;
                _config.Save();
                ApplyProtection();
            };
            _miPause = new ToolStripMenuItem(Lang.T("m_pause"));
            _miPause.Click += (s, e) => TogglePause();
            _miLockRes = new ToolStripMenuItem(Lang.T("m_lockres"));
            _miLockRes.Click += (s, e) => LockResolution();
            _miRestoreRes = new ToolStripMenuItem(Lang.T("m_restore"));
            _miRestoreRes.Click += (s, e) => RestoreResolution();
            _miExclude = new ToolStripMenuItem(Lang.T("m_exclude"));
            _miExclude.Click += (s, e) => ExcludeForeground();
            // The label advertises the language you will switch TO.
            _miLang = new ToolStripMenuItem(Lang.T("m_language"));
            _miLang.Click += (s, e) => SwitchLanguage();
            var miDiagnostics = new ToolStripMenuItem(Lang.T("m_diagnostics"), null, (s, e) => ShowDiagnostics());
            var miAbout = new ToolStripMenuItem(Lang.T("m_about"), null, (s, e) => ShowAbout());
            var miExit = new ToolStripMenuItem(Lang.T("m_exit"), null, (s, e) => Shutdown());

            _menu.Items.AddRange(new ToolStripItem[]
            {
                miSettings, miDraw, new ToolStripSeparator(),
                _miEnable, _miPause, new ToolStripSeparator(),
                _miLockRes, _miRestoreRes, _miExclude, _miLang, new ToolStripSeparator(),
                miDiagnostics, miAbout, miExit
            });
            _menu.Opening += (s, e) => UpdateMenu();
            _tray.ContextMenuStrip = _menu;
            _tray.DoubleClick += (s, e) => OpenSettings();
        }

        private void UpdateMenu()
        {
            _miEnable.Checked = _config.ProtectionEnabled && !_clamper.Paused;
            _miPause.Checked = _clamper.Paused;
            _miPause.Text = _clamper.Paused ? Lang.T("m_resume") : Lang.T("m_pause");
            _miRestoreRes.Enabled = _config.OriginalWidth > 0;
            _miLockRes.Enabled = _config.ProtectionEnabled;
            _miLang.Text = Lang.T("m_language");
            var cur = _resolution.GetCurrent();
            _miLockRes.Text = (_config.LockedWidth > 0 && cur.Width == _config.LockedWidth)
                ? Lang.T("m_lockres_on", _config.LockedWidth, _config.LockedHeight)
                : Lang.T("m_lockres");
        }

        private void SwitchLanguage()
        {
            var code = Lang.Toggle();
            _config.Language = code;
            _config.Save();

            // Rebuild the whole menu in the new language rather than patching entries.
            // The old icon must go first, or two tray entries coexist until GC.
            try { _tray.Visible = false; _tray.Dispose(); } catch { }
            _menu.Dispose();
            BuildTray();
            ApplyProtection();
        }

        private Icon MakeIcon()
        {
            // The tray art lives in UI/Asset/Tray_icon.png and is embedded as a
            // resource, so there is no loose file to lose. Drawn at runtime to
            // whatever size the shell currently asks for.
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("BlindSpot.UI.Asset.Tray_icon.png"))
            {
                if (stream != null)
                {
                    var src = new Bitmap(stream);
                    var bmp = new Bitmap(32, 32);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.Clear(Color.Transparent);
                        g.DrawImage(src, 0, 0, 32, 32);
                    }
                    src.Dispose();
                    return Icon.FromHandle(bmp.GetHicon());
                }
            }
            // Fallback: a plain blue shield if the embedded art is ever missing.
            var fb = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(fb))
            {
                g.Clear(Color.Transparent);
                using (var b = new SolidBrush(Color.FromArgb(30, 120, 210)))
                    g.FillRectangle(b, 6, 4, 20, 24);
            }
            return Icon.FromHandle(fb.GetHicon());
        }

        // (Re)applies every enabled subsystem after a config change or a toggle.
        public void ApplyProtection()
        {
            _config.Save();   // persist whatever we are about to apply
            if (_config.ProtectionEnabled)
            {
                if (_config.ClampWindows) _clamper.Start(); else _clamper.Stop();
                _overlay.Apply();   // registers the AppBar bands + black cover
                UpdateTrayText(Lang.T("tray_active"));
            }
            else
            {
                _clamper.Stop();
                _overlay.Destroy();  // unregisters the AppBars
                UpdateTrayText(Lang.T("tray_inactive"));
            }
            UpdateMenu();
        }

        private void UpdateTrayText(string text)
        {
            if (_tray == null) return;
            try { _tray.Text = text.Length > 63 ? text.Substring(0, 63) : text; } catch { }
        }

        private void TogglePause()
        {
            _clamper.Paused = !_clamper.Paused;
            UpdateTrayText(_clamper.Paused ? Lang.T("tray_paused") : Lang.T("tray_active"));
            UpdateMenu();
        }

        private void OpenSettings()
        {
            using (var f = new SettingsForm(_config, ApplyProtection))
            {
                f.ShowDialog(this);
            }
            _clamper.ResetCycle();
        }

        private void OpenDrawer()
        {
            var f = new ZoneOverlayForm(_config, ApplyProtection);
            f.Show(this);
        }

        private void LockResolution()
        {
            string err = _resolution.LockForExclusiveGames(_config);
            if (err != null)
            {
                MessageBox.Show(this, err, Lang.T("res_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cur = _resolution.GetCurrent();
            DialogResult dr = MessageBox.Show(this,
                Lang.T("res_done", cur.Width, cur.Height),
                Lang.T("res_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateMenu();
        }

        private void RestoreResolution()
        {
            string err = _resolution.RestoreOriginal(_config);
            if (err != null)
                MessageBox.Show(this, err, Lang.T("res_restore_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _config.OriginalWidth = 0;
            _config.OriginalHeight = 0;
            _config.LockedWidth = 0;
            _config.LockedHeight = 0;
            _config.Save();
            UpdateMenu();
        }

        private void ExcludeForeground()
        {
            try
            {
                IntPtr fg = Win32.GetForegroundWindow();
                if (fg == IntPtr.Zero) return;
                Win32.GetWindowThreadProcessId(fg, out uint pid);
                string name = Process.GetProcessById((int)pid).ProcessName;
                if (!_config.IsProcessExcluded(name))
                {
                    _config.ExcludedProcesses.Add(name);
                    _config.Save();
                }
                _clamper.ResetCycle();
                MessageBox.Show(this, Lang.T("excl_added", name), Lang.T("excl_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Lang.T("excl_err", ex.Message), Lang.T("excl_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDisplayChanged(object sender, EventArgs e)
        {
            // A game flipped the display into exclusive mode (or something reset the
            // mode). Re-assert the AppBar bands; the shell drops appbar state across
            // a mode flip, so this is where the protection would otherwise vanish.
            if (!_config.ProtectionEnabled) return;
            _overlay.Apply();

            if (_config.AutoRelockResolution && _config.LockedWidth > 0)
            {
                // Debounce hard: games flip modes repeatedly, and re-applying on every
                // flip would start a ping-pong with the game's own mode setting.
                if ((DateTime.Now - _lastRelock).TotalSeconds > 15)
                {
                    _lastRelock = DateTime.Now;
                    var cur = _resolution.GetCurrent();
                    if (cur.Width != _config.LockedWidth)
                        _resolution.LockForExclusiveGames(_config);
                }
            }
        }

        private void ShowDiagnostics()
        {
            var cur = _resolution.GetCurrent();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Lang.T("diag_current", cur));
            var modes = _resolution.EnumerateModes();
            sb.AppendLine(Lang.T("diag_modes", modes.Count));
            sb.AppendLine();
            sb.AppendLine(Lang.T("diag_lockable"));
            foreach (var m in modes)
            {
                if (m.Height != cur.Height || m.Frequency != cur.Frequency) continue;
                if (m.Width >= cur.Width) continue;
                int leftBar = (cur.Width - m.Width) / 2;
                bool works = leftBar >= _config.MarginLeft;
                sb.AppendLine(Lang.T("diag_bar", m.Width, m.Height, leftBar,
                    works ? Lang.T("diag_ok") : Lang.T("diag_no")));
            }
            sb.AppendLine();
            sb.AppendLine(Lang.T("diag_margins", _config.MarginLeft, _config.MarginRight, _config.MarginTop, _config.MarginBottom));
            MessageBox.Show(this, sb.ToString(), Lang.T("diag_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout()
        {
            MessageBox.Show(this, Lang.T("about_body"), Lang.T("about_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Shutdown()
        {
            _clamper.Stop();
            _refreshTimer.Stop();
            _overlay.Destroy();   // unregisters AppBars so the shell reclaims the space
            if (_config.OriginalWidth > 0)
                _resolution.RestoreOriginal(_config);
            _tray.Visible = false;
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // The form never has a visible window; hide instead of exiting.
                e.Cancel = true;
                Visible = false;
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
