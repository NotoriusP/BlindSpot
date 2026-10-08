using System;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Declared here (not in the manifest) so .NET 8's analyser is happy and the
            // forms receive real per-monitor coordinates.
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Self-test hook: shows the zone-drawing overlay and writes its measured
            // bounds to a file, so the fullscreen-coverage bug can be verified without
            // driving the tray menu.
            if (args != null && args.Length > 0 && args[0] == "--test-overlay")
            {
                var cfg = Core.AppConfig.Load();
                var f = new UI.ZoneOverlayForm(cfg, null);
                f.Load += (s, e) =>
                {
                    var b = f.Bounds;
                    var sc = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                    string report = $"form bounds: {b}\r\nscreen: {sc}\r\nmatches: {b.Width == sc.Width && b.Height == sc.Height}\r\nopacity: {f.Opacity}\r\nAutoScaleMode: {f.AutoScaleMode}";
                    System.IO.File.WriteAllText(
                        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BlindSpot-overlay-test.txt"),
                        report);
                };
                Application.Run(f);
                return;
            }

            Application.Run(new UI.TrayForm());
        }
    }
}
