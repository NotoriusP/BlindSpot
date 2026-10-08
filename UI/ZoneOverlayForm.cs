using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlindSpot.Core;

namespace BlindSpot.UI
{
    // Fullscreen, translucent overlay on which the user drags a rectangle over the
    // damaged area, DesktopCoral-style. Rectangles hugging an edge become the
    // margin for that edge. Live readout shows the resulting geometry in pixels.
    public class ZoneOverlayForm : Form
    {
        private readonly AppConfig _config;
        private readonly Action _onApplied;

        private readonly List<Rectangle> _drawn = new List<Rectangle>();
        private Point _dragStart;
        private Rectangle _currentDrag;
        private bool _isDragging;

        private Button _btnApply, _btnClear, _btnCancel;
        private Label _status;

        public ZoneOverlayForm(AppConfig config, Action onApplied)
        {
            _config = config;
            _onApplied = onApplied;

            bool rtl = Lang.IsRtl;
            RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
            Font = new Font("Tahoma", 9.75f);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Opacity = 0.55;
            BackColor = Color.Black;
            // Critical under PerMonitorV2: the default Font auto-scale would resize the
            // form to a scaled logical size (the "800x800 black box" symptom) instead of
            // the full physical screen.
            AutoScaleMode = AutoScaleMode.None;
            AutoScaleDimensions = new SizeF(96f, 96f);

            // Seed the overlay with the currently configured bands so the user edits
            // an existing setup instead of starting blind.
            var screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                foreach (var z in ZoneMath.GetDeadZones(_config, screen))
                    _drawn.Add(z);
            }

            BuildUi();
            Load += (s, e) =>
            {
                if (screen != null)
                    Bounds = screen.Bounds;
                UpdateStatus();
            };
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
                else if (e.KeyCode == Keys.Enter) OnApply(null, null);
            };
        }

        private void BuildUi()
        {
            _btnApply = new Button { Text = Lang.T("draw_apply"), Size = new Size(110, 30), BackColor = Color.FromArgb(40, 160, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnClear = new Button { Text = Lang.T("draw_clear"), Size = new Size(90, 30), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnCancel = new Button { Text = Lang.T("draw_cancel"), Size = new Size(90, 30), BackColor = Color.FromArgb(160, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            _btnApply.Click += OnApply;
            _btnClear.Click += (s, e) => { _drawn.Clear(); UpdateStatus(); Invalidate(); };
            _btnCancel.Click += (s, e) => Close();

            var bar = new Panel
            {
                BackColor = Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Top,
                Height = 44
            };
            bar.Controls.AddRange(new Control[] { _btnCancel, _btnClear, _btnApply });
            _btnApply.Dock = DockStyle.Right;
            _btnClear.Dock = DockStyle.Right;
            _btnCancel.Dock = DockStyle.Right;
            bar.Padding = new Padding(8, 7, 8, 7);

            _status = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            bar.Controls.Add(_status);
            _status.SendToBack();

            Controls.Add(bar);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            // Ignore presses that land on the toolbar.
            if (e.Y < 50) return;
            _isDragging = true;
            _dragStart = e.Location;
            _currentDrag = new Rectangle(e.Location, Size.Empty);
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_isDragging) return;
            int x = Math.Min(_dragStart.X, e.X);
            int y = Math.Min(_dragStart.Y, e.Y);
            int w = Math.Abs(e.X - _dragStart.X);
            int h = Math.Abs(e.Y - _dragStart.Y);
            _currentDrag = new Rectangle(x, y, w, h);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            Capture = false;
            // Snap to the screen edges so a sloppy drag still registers as "left band".
            var sb = Bounds;
            if (_currentDrag.Width > 12 && _currentDrag.Height > 12)
            {
                var r = _currentDrag;
                // A real damage band is a strip, not a wall. A rectangle covering
                // more than half the screen is a misclick (a full-screen drag sets
                // every margin at once and the four resulting bands fight the shell),
                // so refuse it instead of producing nonsense geometry.
                long area = (long)r.Width * r.Height;
                long screenArea = (long)sb.Width * sb.Height;
                if (screenArea > 0 && area * 2 > screenArea)
                {
                    _status.Text = "  " + Lang.T("draw_toobig");
                    _currentDrag = Rectangle.Empty;
                    Invalidate();
                    return;
                }
                if (r.Left <= 24) { r.X = sb.Left; }
                if (r.Top <= 24) { r.Y = sb.Top; }
                if (r.Right >= sb.Right - 24) { r.Width = sb.Right - r.Left; }
                if (r.Bottom >= sb.Bottom - 24) { r.Height = sb.Bottom - r.Top; }
                _drawn.Add(r);
            }
            _currentDrag = Rectangle.Empty;
            UpdateStatus();
            Invalidate();
        }

        private void UpdateStatus()
        {
            var m = ComputeMargins();
            _status.Text = "  " + Lang.T("draw_status", m.left, m.right, m.top, m.bottom, _drawn.Count) +
                           "      " + Lang.T("draw_hint");
        }

        // Each drawn rectangle is a band on exactly ONE edge. The edge it belongs to
        // is the one it hugs whose perpendicular thickness is smallest. This matters
        // for a full-height strip down the left side: it hugs the left, top AND
        // bottom edges at once, but as a left band it is 158px thick while as a
        // top/bottom band it is the whole screen tall — so it is unambiguously a LEFT
        // band. Counting it for every edge it touches is what wrote MarginTop=1080
        // and MarginBottom=1080 into the config from a single left-edge drag.
        private (int left, int right, int top, int bottom) ComputeMargins()
        {
            var sb = Bounds;
            int left = 0, right = 0, top = 0, bottom = 0;
            const int snap = 24;
            foreach (var r in _drawn)
            {
                int distL = Math.Abs(r.Left - sb.Left);
                int distR = Math.Abs(r.Right - sb.Right);
                int distT = Math.Abs(r.Top - sb.Top);
                int distB = Math.Abs(r.Bottom - sb.Bottom);
                int thickH = r.Width;    // thickness if this were a left/right band
                int thickV = r.Height;   // thickness if this were a top/bottom band

                int bestThick = int.MaxValue;
                int edge = -1;
                if (distL <= snap && thickH < bestThick) { bestThick = thickH; edge = 0; }
                if (distR <= snap && thickH < bestThick) { bestThick = thickH; edge = 1; }
                if (distT <= snap && thickV < bestThick) { bestThick = thickV; edge = 2; }
                if (distB <= snap && thickV < bestThick) { bestThick = thickV; edge = 3; }
                if (edge < 0) continue;

                switch (edge)
                {
                    case 0: left = Math.Max(left, r.Right - sb.Left); break;
                    case 1: right = Math.Max(right, sb.Right - r.Left); break;
                    case 2: top = Math.Max(top, r.Bottom - sb.Top); break;
                    case 3: bottom = Math.Max(bottom, sb.Bottom - r.Top); break;
                }
            }
            // Report exactly what ZoneMath will honour. Without this the readout could
            // promise a band that gets silently clamped, so the user would save and
            // get a different protection than the one they drew.
            var screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                var probe = new AppConfig
                {
                    MarginLeft = left, MarginRight = right,
                    MarginTop = top, MarginBottom = bottom
                };
                return ZoneMath.ClampedMargins(probe, screen);
            }
            return (left, right, top, bottom);
        }

        private void OnApply(object s, EventArgs e)
        {
            var m = ComputeMargins();
            _config.MarginLeft = m.left;
            _config.MarginRight = m.right;
            _config.MarginTop = m.top;
            _config.MarginBottom = m.bottom;
            _config.ProtectionEnabled = true;
            _config.Save();

            // Get this fullscreen black overlay off the screen BEFORE registering the
            // AppBars. If we apply while still visible, the user sees the whole screen
            // go black, and if the registration throws we stay black forever — that is
            // the "whole screen went black and it crashed" symptom.
            Hide();
            try { _onApplied?.Invoke(); }
            finally { Close(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var r in _drawn)
            {
                using (var b = new SolidBrush(Color.FromArgb(90, 200, 60, 60)))
                    g.FillRectangle(b, r);
                using (var p = new Pen(Color.Red, 2))
                    g.DrawRectangle(p, r);
                DrawSize(g, r);
            }

            if (_isDragging && !_currentDrag.IsEmpty)
            {
                using (var b = new SolidBrush(Color.FromArgb(90, 90, 140, 220)))
                    g.FillRectangle(b, _currentDrag);
                using (var p = new Pen(Color.DodgerBlue, 2) { DashStyle = DashStyle.Dash })
                    g.DrawRectangle(p, _currentDrag);
                DrawSize(g, _currentDrag);
            }
        }

        private void DrawSize(Graphics g, Rectangle r)
        {
            if (r.Width < 30 || r.Height < 20) return;
            string text = $"{r.Width}x{r.Height}px";
            using (var f = new Font("Tahoma", 10f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sz = g.MeasureString(text, f);
                var loc = new PointF(r.Left + 6, r.Top + 4);
                using (var bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    g.FillRectangle(bg, loc.X, loc.Y, sz.Width + 8, sz.Height + 4);
                g.DrawString(text, f, b, loc.X + 4, loc.Y + 2);
            }
        }
    }
}
