using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    // Converts the configured edge margins into concrete screen geometry:
    //   * SafeRect    - the usable inner rectangle (where apps should live)
    //   * DeadZones   - the damaged bands that must stay black/empty
    // All coordinates are physical screen pixels.
    public static class ZoneMath
    {
        // A mistyped or misdrawn margin must never swallow the desktop. Real damage
        // strips are thin, so cap each band at a generous but finite physical size.
        private const int MaxBandPx = 480;

        public static (int left, int right, int top, int bottom) ClampedMargins(AppConfig config, Screen screen)
        {
            var b = screen.Bounds;
            int maxH = System.Math.Min(MaxBandPx, b.Width * 35 / 100);
            int maxV = System.Math.Min(MaxBandPx, b.Height * 35 / 100);
            return (Clamp(config.MarginLeft, maxH),
                    Clamp(config.MarginRight, maxH),
                    Clamp(config.MarginTop, maxV),
                    Clamp(config.MarginBottom, maxV));
        }

        private static int Clamp(int v, int max)
        {
            if (v < 0) return 0;
            if (v > max) return max;
            return v;
        }

        public static Rectangle GetSafeRect(AppConfig config, Screen screen)
        {
            var b = screen.Bounds;
            var m = ClampedMargins(config, screen);
            int x = b.X + m.left;
            int y = b.Y + m.top;
            int w = b.Width - m.left - m.right;
            int h = b.Height - m.top - m.bottom;
            if (w < 200) w = 200;   // refuse to collapse to nothing
            if (h < 200) h = 200;
            return new Rectangle(x, y, w, h);
        }

        public static List<Rectangle> GetDeadZones(AppConfig config, Screen screen)
        {
            var b = screen.Bounds;
            var m = ClampedMargins(config, screen);
            var zones = new List<Rectangle>();
            if (m.left > 0)
                zones.Add(new Rectangle(b.X, b.Y, m.left, b.Height));
            if (m.top > 0)
                zones.Add(new Rectangle(b.X, b.Y, b.Width, m.top));
            if (m.right > 0)
                zones.Add(new Rectangle(b.Right - m.right, b.Y, m.right, b.Height));
            if (m.bottom > 0)
                zones.Add(new Rectangle(b.X, b.Bottom - m.bottom, b.Width, m.bottom));
            return zones;
        }

        // True when the window covers a meaningful slice of a dead band. The
        // tolerance stops shadows / 1px borders from triggering a fight with the user.
        public static bool ViolatesZones(RECT windowRect, List<Rectangle> zones, int tolerance)
        {
            var wr = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Width, windowRect.Height);
            foreach (var z in zones)
            {
                var inter = Rectangle.Intersect(wr, z);
                if (inter.Width > tolerance || inter.Height > tolerance)
                    return true;
            }
            return false;
        }

        public static bool IsInsideSafeZone(RECT windowRect, Rectangle safeRect)
        {
            return windowRect.Left >= safeRect.Left - 2
                && windowRect.Top >= safeRect.Top - 2
                && windowRect.Right <= safeRect.Right + 2
                && windowRect.Bottom <= safeRect.Bottom + 2;
        }

        // Marshal a native rect straight from GetWindowRect into the managed geometry
        // the rest of the clamping logic speaks.
        public static Rectangle ScreenRectToNative(RECT r)
        {
            return new Rectangle(r.Left, r.Top, r.Width, r.Height);
        }
    }
}
