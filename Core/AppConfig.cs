using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BlindSpot.Core
{
    // Margins define the damaged/dead bands on each edge of the screen, in physical
    // pixels. Everything inside the resulting inner rectangle is the "safe zone".
    public class AppConfig
    {
        public bool ProtectionEnabled { get; set; } = true;
        // "fa" or "en". Anything unrecognised falls back to Persian.
        public string Language { get; set; } = "fa";
        public int MarginLeft { get; set; } = 120;
        public int MarginRight { get; set; } = 0;
        public int MarginTop { get; set; } = 0;
        public int MarginBottom { get; set; } = 0;

        public bool ClampWindows { get; set; } = true;
        public bool LockWorkArea { get; set; } = true;
        public bool ShowBandOverlay { get; set; } = true;
        public bool AutoRelockResolution { get; set; } = false;

        // Process names (without .exe) that must never be clamped, e.g. a game that
        // misbehaves when resized.
        public List<string> ExcludedProcesses { get; set; } = new List<string>();

        // Snapshot of the display mode before we applied a reduced resolution, so we
        // can restore it exactly.
        public int OriginalWidth { get; set; } = 0;
        public int OriginalHeight { get; set; } = 0;
        public int OriginalFrequency { get; set; } = 0;
        public int LockedWidth { get; set; } = 0;
        public int LockedHeight { get; set; } = 0;

        private const string ConfigFileName = "config.json";

        public static AppConfig Load()
        {
            string path = GetPath();
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch
            {
                // Corrupt or unreadable config: fall back to defaults. Never crash the
                // tray app over a settings file.
            }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(GetPath());
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetPath(), json);
            }
            catch
            {
                // Best-effort persistence.
            }
        }

        private static string GetPath()
        {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlindSpot");
                return Path.Combine(dir, ConfigFileName);
        }

        public bool IsProcessExcluded(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            foreach (string ex in ExcludedProcesses)
            {
                if (string.Equals(ex, processName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
