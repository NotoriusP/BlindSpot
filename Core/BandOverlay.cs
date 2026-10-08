using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlindSpot.Native;

namespace BlindSpot.Core
{
    // Paints an opaque black band over each dead zone AND registers it with the
    // shell as an AppBar, so Windows reserves the space (icons move, maximised
    // windows shrink). One window does both jobs.
    public class BandOverlay
    {
        private readonly List<AppBarBand> _bands = new List<AppBarBand>();
        private readonly AppConfig _config;
        private bool _applied;

        public BandOverlay(AppConfig config)
        {
            _config = config;
        }

        public void Apply()
        {
            var screen = Screen.PrimaryScreen;
            if (screen == null) return;

            if (!_config.ShowBandOverlay)
            {
                if (_applied) Destroy();
                return;
            }

            var desired = ZoneMath.GetDeadZones(_config, screen);

            // Idempotent path: if the live bands already cover exactly the zones we
            // want, do nothing. Destroying and re-creating an AppBar makes the shell
            // snap the work area back and then forward again, which flickers the
            // black band and shakes every maximised window. The 5-second safety timer
            // hits this path constantly, so it must be free.
            if (_applied && BandsMatch(desired)) return;

            Destroy();
            foreach (var z in desired)
            {
                int edge = EdgeForZone(z, screen.Bounds);
                var band = new AppBarBand(_config, z, edge);
                _bands.Add(band);
                band.Register();
            }
            _applied = true;
        }

        // Bands are current when there is exactly one live, registered band per
        // desired zone, each sitting on the rect we would ask for. Anything else —
        // a dead window, a dropped registration, a resolution change that moved a
        // band — means we must rebuild.
        private bool BandsMatch(List<Rectangle> desired)
        {
            if (_bands.Count != desired.Count) return false;
            for (int i = 0; i < desired.Count; i++)
            {
                var b = _bands[i];
                if (!b.IsAlive || !b.IsRegistered) return false;
                if (b.BandRect != desired[i]) return false;
            }
            return true;
        }

        // The shell wants the edge the band hugs, so it knows which dimension to
        // reserve. A band touching the left screen edge is ABE_LEFT, etc.
        private int EdgeForZone(Rectangle z, Rectangle screen)
        {
            if (z.Left <= screen.Left + 2) return Win32.ABE_LEFT;
            if (z.Right >= screen.Right - 2) return Win32.ABE_RIGHT;
            if (z.Top <= screen.Top + 2) return Win32.ABE_TOP;
            return Win32.ABE_BOTTOM;
        }

        public void Destroy()
        {
            foreach (var b in _bands)
            {
                try { b.Unregister(); b.Close(); b.Dispose(); } catch { }
            }
            _bands.Clear();
            _applied = false;
        }

        public bool IsApplied => _applied;
    }
}
