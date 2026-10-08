using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    public class DisplayMode
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frequency { get; set; }
        public int BitsPerPixel { get; set; }

        public override string ToString() => $"{Width}x{Height} @{Frequency}Hz";
    }

    // Handles the "resolution lock" for exclusive-fullscreen games.
    //
    // An exclusive-fullscreen game owns the scan-out path, so no user-mode program
    // can crop its image. What we CAN do is switch the whole panel to a narrower
    // mode and ask the GPU to centre it (DMDFO_CENTER) instead of stretching it.
    // The result: a sharp 1:1 image with black bars, never touching the damaged
    // left band. Centreing is symmetric, so the panel loses (2 x margin) px of width.
    public class ResolutionManager
    {
        private readonly string _deviceName;

        public ResolutionManager()
        {
            _deviceName = Screen.PrimaryScreen != null ? Screen.PrimaryScreen.DeviceName : null;
        }

        public DisplayMode GetCurrent()
        {
            var dm = new DEVMODE();
            dm.dmSize = 220;
            Win32.EnumDisplaySettingsEx(_deviceName, Win32.ENUM_CURRENT_SETTINGS, ref dm, 0);
            return new DisplayMode { Width = dm.dmPelsWidth, Height = dm.dmPelsHeight, Frequency = dm.dmDisplayFrequency, BitsPerPixel = (int)dm.dmBitsPerPel };
        }

        public List<DisplayMode> EnumerateModes()
        {
            var modes = new List<DisplayMode>();
            var seen = new HashSet<string>();
            var dm = new DEVMODE();
            dm.dmSize = 220;
            for (int i = 0; i < 400; i++)
            {
                if (Win32.EnumDisplaySettingsEx(_deviceName, i, ref dm, 0) == 0)
                    break;
                if (dm.dmPelsWidth < 800 || dm.dmPelsHeight < 600)
                    continue;
                if (dm.dmDisplayFrequency < 50)
                    continue;
                string key = $"{dm.dmPelsWidth}x{dm.dmPelsHeight}x{dm.dmDisplayFrequency}";
                if (seen.Add(key))
                {
                    modes.Add(new DisplayMode
                    {
                        Width = dm.dmPelsWidth,
                        Height = dm.dmPelsHeight,
                        Frequency = dm.dmDisplayFrequency,
                        BitsPerPixel = (int)dm.dmBitsPerPel
                    });
                }
            }
            return modes;
        }

        // Picks the largest available mode whose centred image clears the left dead
        // band. Priority:
        //   1. same height & refresh as native (ideal: only side bars)
        //   2. same aspect ratio & refresh (e.g. 1600x900 for a 1920x1080 panel)
        //   3. anything with the right refresh that fits
        // Custom timings (1680x1080 etc.) are not tried here: laptop eDP panels reject
        // them with DISP_CHANGE_BADMODE, so we work with the modes the driver exposes.
        public DisplayMode PickReducedMode(AppConfig config)
        {
            var current = GetCurrent();
            int maxWidth = current.Width - 2 * config.MarginLeft;
            if (maxWidth < 800)
                maxWidth = 800;

            var modes = EnumerateModes();

            DisplayMode best = null;
            int bestPriority = -1;
            double nativeAspect = (double)current.Width / current.Height;

            foreach (var m in modes)
            {
                if (m.Frequency != current.Frequency)
                    continue;
                if (m.Width >= current.Width)
                    continue;
                if (m.Width > maxWidth)
                    continue;

                // Centred left bar must hide the damage.
                int leftBar = (current.Width - m.Width) / 2;
                if (leftBar < config.MarginLeft)
                    continue;

                int priority;
                if (m.Height == current.Height)
                    priority = 3;
                else if (Math.Abs((double)m.Width / m.Height - nativeAspect) < 0.02)
                    priority = 2;
                else
                    priority = 1;

                // Highest priority wins; within a priority, widest wins.
                if (best == null || priority > bestPriority ||
                    (priority == bestPriority && m.Width > best.Width))
                {
                    best = m;
                    bestPriority = priority;
                }
            }
            return best;
        }

        // Applies `mode` centred on the panel. Returns a human-readable result.
        public string ApplyMode(DisplayMode mode)
        {
            var dm = new DEVMODE();
            dm.dmSize = 220;
            dm.dmFields = Win32.DM_PELSWIDTH | Win32.DM_PELSHEIGHT |
                          Win32.DM_BITSPERPEL | Win32.DM_DISPLAYFREQUENCY |
                          Win32.DM_DISPLAYFIXEDOUTPUT;
            dm.dmPelsWidth = mode.Width;
            dm.dmPelsHeight = mode.Height;
            dm.dmDisplayFrequency = mode.Frequency;
            dm.dmBitsPerPel = mode.BitsPerPixel > 0 ? mode.BitsPerPixel : 32;
            dm.dmDisplayFixedOutput = Win32.DMDFO_CENTER;   // <- no stretch, black bars

            int ret = Win32.ChangeDisplaySettingsEx(_deviceName, ref dm, IntPtr.Zero,
                Win32.CDS_UPDATEREGISTRY | Win32.CDS_NORESET, IntPtr.Zero);
            if (ret != Win32.DISP_CHANGE_SUCCESSFUL)
                return $"خطا در ذخیره‌ی حالت نمایش (کد {ret})";

            ret = Win32.ChangeDisplaySettingsEx(_deviceName, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);
            if (ret != Win32.DISP_CHANGE_SUCCESSFUL)
                return $"خطا در اعمال حالت نمایش (کد {ret})";

            return null;
        }

        public string RestoreOriginal(AppConfig config)
        {
            if (config.OriginalWidth <= 0 || config.OriginalHeight <= 0)
                return "حالت اصلی ذخیره نشده بود.";
            var dm = new DEVMODE();
            dm.dmSize = 220;
            dm.dmFields = Win32.DM_PELSWIDTH | Win32.DM_PELSHEIGHT |
                          Win32.DM_BITSPERPEL | Win32.DM_DISPLAYFREQUENCY |
                          Win32.DM_DISPLAYFIXEDOUTPUT;
            dm.dmPelsWidth = config.OriginalWidth;
            dm.dmPelsHeight = config.OriginalHeight;
            dm.dmDisplayFrequency = config.OriginalFrequency > 0 ? config.OriginalFrequency : 60;
            dm.dmBitsPerPel = config.OriginalBitsPerPixel > 0 ? config.OriginalBitsPerPixel : 32;
            dm.dmDisplayFixedOutput = Win32.DMDFO_DEFAULT;

            int ret = Win32.ChangeDisplaySettingsEx(_deviceName, ref dm, IntPtr.Zero,
                Win32.CDS_UPDATEREGISTRY | Win32.CDS_NORESET, IntPtr.Zero);
            if (ret != Win32.DISP_CHANGE_SUCCESSFUL)
                return $"خطا در بازیابی (کد {ret})";
            ret = Win32.ChangeDisplaySettingsEx(_deviceName, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);
            if (ret != Win32.DISP_CHANGE_SUCCESSFUL)
                return $"خطا در اعمال بازیابی (کد {ret})";
            return null;
        }

        // Snapshot the current mode so it can be restored later, then apply the
        // reduced centred mode. Returns an error string or null on success.
        public string LockForExclusiveGames(AppConfig config)
        {
            var current = GetCurrent();
            var reduced = PickReducedMode(config);
            if (reduced == null)
                return "هیچ حالت نمایشی برای این حاشیه پیدا نشد. حاشیه را کمتر کنید.";

            // Snapshot into locals first and only touch the live config once the
            // switch is verified. Mutating config up front means a later
            // ApplyProtection() -> Save() would persist a lock that never happened.
            int origW = current.Width, origH = current.Height, origHz = current.Frequency, origBpp = current.BitsPerPixel;

            string err = ApplyMode(reduced);
            if (err != null)
                return err;

            // Verify the switch actually took effect.
            var now = GetCurrent();
            if (now.Width != reduced.Width || now.Height != reduced.Height)
                return "تغییر اعمال شد ولی کارت گرافیک آن را نپذیرفت (احتمالاً نیاز به تنظیم Center در Intel Graphics Command Center است).";

            config.OriginalWidth = origW;
            config.OriginalHeight = origH;
            config.OriginalFrequency = origHz;
            config.OriginalBitsPerPixel = origBpp;
            config.LockedWidth = reduced.Width;
            config.LockedHeight = reduced.Height;
            config.Save();
            return null;
        }
    }
}
