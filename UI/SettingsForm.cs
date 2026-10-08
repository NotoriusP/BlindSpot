using System;
using System.Drawing;
using System.Windows.Forms;
using BlindSpot.Core;

namespace BlindSpot.UI
{
    // Numeric setup for the damaged bands plus feature toggles. Bilingual: every label
    // comes from Lang.T.
    //
    // LAYOUT NOTE: the form hosts a TableLayoutPanel rather than hand-positioned
    // controls. Under RightToLeftLayout the X axis is measured from the RIGHT edge, so
    // manual coordinates pushed long labels ("???????? (??? ?????? ?? ????)") past the
    // left edge where they clipped, and put the Cancel button at x=435 on a 430px form —
    // off the client area entirely. The table mirrors its own columns under RTL, so
    // nothing can fall off the form regardless of label length.
    public class SettingsForm : Form
    {
        private readonly AppConfig _config;
        private readonly Action _onApplied;

        private NumericUpDown _left, _right, _top, _bottom;
        private CheckBox _chkClamp, _chkOverlay, _chkAutoRelock;
        private TextBox _exclusions;
        private Label _hintLabel;
        private Button _apply, _cancel;

        public SettingsForm(AppConfig config, Action onApplied)
        {
            _config = config;
            _onApplied = onApplied;

            Font = new Font("Tahoma", 9.75f);
            Text = Lang.T("settings_title");
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(470, 530);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildUi();
            LoadConfig();
        }

        private void BuildUi()
        {
            bool rtl = Lang.IsRtl;
            RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = rtl;

            const int btnStrip = 48;

            var table = new TableLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, ClientSize.Height - btnStrip),
                ColumnCount = 2,
                Padding = new Padding(16, 10, 16, 6),
                RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No
            };
            // Column 0: labels, sized to their widest text. Column 1: inputs, fixed.
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            Controls.Add(table);

            int row = 0;

            // A bold header spanning the full width.
            void Section(string key)
            {
                var l = new Label
                {
                    Text = Lang.T(key),
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 8, 0, 4)
                };
                table.Controls.Add(l, 0, row);
                table.SetColumnSpan(l, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                row++;
            }

            // A label + numeric up-down on one row.
            NumericUpDown NumericRow(string key)
            {
                var l = new Label
                {
                    Text = Lang.T(key),
                    AutoSize = true,
                    Margin = new Padding(0, 5, 10, 5),
                    Anchor = AnchorStyles.Top
                };
                var nud = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 800,
                    Width = 80,
                    RightToLeft = RightToLeft.No,
                    Margin = new Padding(0, 3, 0, 3),
                    Anchor = AnchorStyles.Top
                };
                l.Click += (s, e) => nud.Focus();
                table.Controls.Add(l, 0, row);
                table.Controls.Add(nud, 1, row);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
                nud.ValueChanged += AnyValueChanged;
                row++;
                return nud;
            }

            // A checkbox using the whole row.
            CheckBox WideCheckbox(string key)
            {
                var cb = new CheckBox
                {
                    Text = Lang.T(key),
                    AutoSize = true,
                    Margin = new Padding(0, 7, 0, 3),
                    Anchor = AnchorStyles.Top
                };
                table.Controls.Add(cb, 0, row);
                table.SetColumnSpan(cb, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                cb.CheckedChanged += AnyValueChanged;
                row++;
                return cb;
            }

            Section("grp_bands");
            _left = NumericRow("lbl_left");
            _right = NumericRow("lbl_right");
            _top = NumericRow("lbl_top");
            _bottom = NumericRow("lbl_bottom");

            Section("grp_features");
            _chkClamp = WideCheckbox("chk_clamp");
            _chkOverlay = WideCheckbox("chk_overlay");
            _chkAutoRelock = WideCheckbox("chk_autorelock");

            Section("lbl_exclusions");
            _exclusions = new TextBox
            {
                Width = 210,
                RightToLeft = RightToLeft.No,
                Margin = new Padding(0, 3, 0, 8),
                Anchor = AnchorStyles.Top
            };
            table.Controls.Add(_exclusions, 0, row);
            table.SetColumnSpan(_exclusions, 2);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            row++;

            // The saved-hint line, on its own row, never touching the buttons.
            // AutoSize=false + a fixed height keeps the table from growing this row
            // (Dock=Fill made the row balloon to 79px and stretch the whole form).
            _hintLabel = new Label
            {
                AutoSize = false,
                Size = new Size(438, 20),
                Margin = new Padding(0, 2, 0, 2),
                ForeColor = Color.OliveDrab,
                TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft
            };
            table.Controls.Add(_hintLabel, 0, row);
            table.SetColumnSpan(_hintLabel, 2);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            row++;

            // Buttons in a flow strip: they wrap/flow inside the strip and can never
            // sit off the client area.
            var buttons = new FlowLayoutPanel
            {
                Location = new Point(0, ClientSize.Height - btnStrip),
                Size = new Size(ClientSize.Width, btnStrip),
                FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                Padding = new Padding(16, 7, 16, 8),
                WrapContents = false,
                AutoScroll = false
            };
            _apply = new Button { Text = Lang.T("btn_apply"), Size = new Size(100, 30) };
            _cancel = new Button { Text = Lang.T("btn_cancel"), Size = new Size(100, 30) };
            _apply.Click += OnApply;
            _cancel.Click += (s, e) => Close();
            // Primary button hugs the trailing edge in both directions.
            if (rtl) { buttons.Controls.Add(_apply); buttons.Controls.Add(_cancel); }
            else { buttons.Controls.Add(_cancel); buttons.Controls.Add(_apply); }
            Controls.Add(buttons);
            AcceptButton = _apply;
            CancelButton = _cancel;
        }

        private void LoadConfig()
        {
            _left.Value = ClampValue(_config.MarginLeft);
            _right.Value = ClampValue(_config.MarginRight);
            _top.Value = ClampValue(_config.MarginTop);
            _bottom.Value = ClampValue(_config.MarginBottom);
            _chkClamp.Checked = _config.ClampWindows;
            _chkOverlay.Checked = _config.ShowBandOverlay;
            _chkAutoRelock.Checked = _config.AutoRelockResolution;
            _exclusions.Text = string.Join(", ", _config.ExcludedProcesses ?? new System.Collections.Generic.List<string>());
        }

        private decimal ClampValue(int v)
        {
            if (v < 0) return 0;
            if (v > 800) return 800;
            return v;
        }

        private void AnyValueChanged(object s, EventArgs e)
        {
            _hintLabel.Text = "";
        }

        private void OnApply(object s, EventArgs e)
        {
            _config.MarginLeft = (int)_left.Value;
            _config.MarginRight = (int)_right.Value;
            _config.MarginTop = (int)_top.Value;
            _config.MarginBottom = (int)_bottom.Value;
            _config.ClampWindows = _chkClamp.Checked;
            _config.ShowBandOverlay = _chkOverlay.Checked;
            _config.AutoRelockResolution = _chkAutoRelock.Checked;

            var names = new System.Collections.Generic.List<string>();
            foreach (var part in _exclusions.Text.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = part.Trim();
                if (t.Length > 0) names.Add(t);
            }
            _config.ExcludedProcesses = names;

            _config.Save();
            _onApplied?.Invoke();
            _hintLabel.Text = Lang.T("hint_saved");
        }
    }
}
