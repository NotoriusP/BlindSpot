using System.Collections.Generic;

namespace BlindSpot.Core
{
    // Bilingual UI strings. Every user-visible label goes through Lang.T(key) so the
    // whole app flips between Persian and English from one place. The tray menu's
    // language item writes AppConfig.Language, and every form rebuilds its text from
    // this table on open.
    public static class Lang
    {
        public const string Fa = "fa";
        public const string En = "en";

        private static readonly Dictionary<string, string> FaTable = new Dictionary<string, string>
        {
            { "tray_title",        "BlindSpot" },
            { "tray_active",       "BlindSpot — فعال" },
            { "tray_inactive",     "BlindSpot — غیرفعال" },
            { "tray_paused",       "BlindSpot — متوقف موقت" },
            { "m_settings",        "تنظیمات ناحیهٔ امن..." },
            { "m_draw",            "رسم ناحیه با ماوس..." },
            { "m_enable",          "محافظت فعال" },
            { "m_pause",           "توقف موقت (Ctrl+Alt+P)" },
            { "m_resume",          "ادامه (Ctrl+Alt+P)" },
            { "m_lockres",         "قفل رزولوشن برای بازی Fullscreen" },
            { "m_lockres_on",      "قفل فعال: {0}x{1}" },
            { "m_restore",         "بازیابی رزولوشن اصلی" },
            { "m_exclude",         "استثنا کردن برنامهٔ فعلی" },
            { "m_diagnostics",     "اطلاعات نمایشگر..." },
            { "m_about",           "درباره و راهنما..." },
            { "m_language",        "زبان: English" },   // shows the OTHER language's name
            { "m_exit",            "خروج" },

            { "settings_title",    "تنظیمات ناحیهٔ امن | BlindSpot" },
            { "grp_bands",         "نوارهای آسیب‌دیده (پیکسل)" },
            { "lbl_left",          "حاشیهٔ چپ" },
            { "lbl_right",         "حاشیهٔ راست" },
            { "lbl_top",           "حاشیهٔ بالا" },
            { "lbl_bottom",        "حاشیهٔ پایین" },
            { "grp_features",      "قابلیت‌ها" },
            { "chk_clamp",         "هل دادن پنجره‌ها به ناحیهٔ امن" },
            { "chk_overlay",       "پوشش سیاه روی نوار آسیب‌دیده (AppBar)" },
            { "chk_autorelock",    "قفل مجدد خودکار رزولوشن (برای بازی‌های Fullscreen)" },
            { "lbl_exclusions",    "استثناها (نام پروسه، با کاما)" },
            { "btn_apply",         "اعمال" },
            { "btn_cancel",        "انصراف" },
            { "hint_saved",        "✓ ذخیره شد" },

            { "draw_apply",        "ذخیره و فعال" },
            { "draw_clear",        "پاک کردن" },
            { "draw_cancel",       "انصراف" },
            { "draw_status",       "چپ: {0}px | راست: {1}px | بالا: {2}px | پایین: {3}px   ({4} مستطیل)" },
            { "draw_hint",         "با ماوس روی ناحیهٔ آسیب‌دیده بکشید — Enter: ذخیره، Esc: انصراف" },
            { "draw_toobig",       "مستطیل خیلی بزرگ است — ناحیهٔ آسیب‌دیده یک نوار باریک است؛ دوباره بکشید" },

            { "res_title",         "قفل رزولوشن" },
            { "res_done",          "رزولوشن روی {0}x{1} مرکزسازی شد.\n\nنوار چپ سیاه می‌ماند.\n\nبرای بازی‌های Fullscreen حقیقی:\n۱) بازی را اجرا کنید\n۲) در تنظیماتِ گرافیکِ بازی، رزولوشن را روی {0}x{1} بگذارید\n   (یا حالت Windowed/Borderless انتخاب کنید)\n\nگزینهٔ «قفل مجدد خودکار» هم اگر فعّال باشد، پس از تعویض حالتِ نمایش، دوباره قفل را اعمال می‌کند.\n\nبعد از اتمام بازی، از منوی راست‌کلیک «بازیابی رزولوشن اصلی» را بزنید." },
            { "res_restore_title", "بازیابی" },
            { "diag_title",        "اطلاعات نمایشگر" },
            { "diag_current",      "نمایشگر فعلی: {0}" },
            { "diag_modes",        "تعداد حالت‌ها: {0}" },
            { "diag_lockable",     "حالت‌های ممکن برای قفل (همان ارتفاع، مرکزسازی):" },
            { "diag_bar",          "  {0}x{1} -> نوار چپ {2}px {3}" },
            { "diag_ok",           "(مناسب)" },
            { "diag_no",           "(کمتر از حاشیه)" },
            { "diag_margins",      "حاشیه‌ها: چپ={0} راست={1} بالا={2} پایین={3}" },

            { "excl_title",        "استثنا" },
            { "excl_added",        "'{0}' به استثناها اضافه شد." },
            { "excl_err",          "خطا: {0}" },

            { "about_title",       "درباره" },
            { "about_body",        "BlindSpot — ناحیهٔ امن برای مانیتور آسیب‌دیده\n\nروش کار:\n۱) ناحیهٔ آسیب‌دیده را رسم یا در تنظیمات به پیکسل وارد کنید.\n۲) برنامه‌ها و پنجره‌ها به ناحیهٔ امن هل داده می‌شوند.\n۳) برای بازی‌های Fullscreen حقیقی، از «قفل رزولوشن» استفاده کنید: رزولوشن کوچک‌تر شده و در مرکز صفحه قرار می‌گیرد تا نوار آسیب‌دیده سیاه بماند.\n\nتوقف موقت: Ctrl+Alt+P\nنکته: اگر بازی به‌درستی هل داده نمی‌شود، BlindSpot را به‌صورت مدیر اجرا کنید." },
        };

        private static readonly Dictionary<string, string> EnTable = new Dictionary<string, string>
        {
            { "tray_title",        "BlindSpot" },
            { "tray_active",       "BlindSpot — active" },
            { "tray_inactive",     "BlindSpot — off" },
            { "tray_paused",       "BlindSpot — paused" },
            { "m_settings",        "Safe zone settings..." },
            { "m_draw",            "Draw zone with mouse..." },
            { "m_enable",          "Protection enabled" },
            { "m_pause",           "Pause (Ctrl+Alt+P)" },
            { "m_resume",          "Resume (Ctrl+Alt+P)" },
            { "m_lockres",         "Lock resolution for fullscreen games" },
            { "m_lockres_on",      "Lock active: {0}x{1}" },
            { "m_restore",         "Restore original resolution" },
            { "m_exclude",         "Exclude the current app" },
            { "m_diagnostics",     "Display diagnostics..." },
            { "m_about",           "About & help..." },
            { "m_language",        "Language: فارسی" },
            { "m_exit",            "Exit" },

            { "settings_title",    "Safe zone settings | BlindSpot" },
            { "grp_bands",         "Damaged bands (pixels)" },
            { "lbl_left",          "Left margin" },
            { "lbl_right",         "Right margin" },
            { "lbl_top",           "Top margin" },
            { "lbl_bottom",        "Bottom margin" },
            { "grp_features",      "Features" },
            { "chk_clamp",         "Push windows into the safe zone" },
            { "chk_overlay",       "Black cover over the damaged band (AppBar)" },
            { "chk_autorelock",    "Auto re-lock resolution (for fullscreen games)" },
            { "lbl_exclusions",    "Exclusions (process name, comma separated)" },
            { "btn_apply",         "Apply" },
            { "btn_cancel",        "Cancel" },
            { "hint_saved",        "✓ Saved" },

            { "draw_apply",        "Save & apply" },
            { "draw_clear",        "Clear" },
            { "draw_cancel",       "Cancel" },
            { "draw_status",       "Left: {0}px | Right: {1}px | Top: {2}px | Bottom: {3}px   ({4} rect(s))" },
            { "draw_hint",         "Drag the mouse over the damaged area — Enter: save, Esc: cancel" },
            { "draw_toobig",       "Rectangle too large — the damaged zone is a narrow band; draw again" },

            { "res_title",         "Resolution lock" },
            { "res_done",          "Resolution centred at {0}x{1}.\n\nThe left band stays black.\n\nFor true fullscreen games:\n1) Launch the game\n2) In its graphics settings set resolution to {0}x{1}\n   (or pick Windowed/Borderless)\n\nIf 'Auto re-lock' is on, the lock is re-applied after each display-mode change.\n\nWhen you finish the game, use 'Restore original resolution' from the right-click menu." },
            { "res_restore_title", "Restore" },
            { "diag_title",        "Display diagnostics" },
            { "diag_current",      "Current display: {0}" },
            { "diag_modes",        "Mode count: {0}" },
            { "diag_lockable",     "Lockable modes (same height, centred):" },
            { "diag_bar",          "  {0}x{1} -> left bar {2}px {3}" },
            { "diag_ok",           "(fits)" },
            { "diag_no",           "(smaller than margin)" },
            { "diag_margins",      "Margins: left={0} right={1} top={2} bottom={3}" },

            { "excl_title",        "Exclusions" },
            { "excl_added",        "'{0}' added to exclusions." },
            { "excl_err",          "Error: {0}" },

            { "about_title",       "About" },
            { "about_body",        "BlindSpot — a safe zone for a damaged monitor\n\nHow it works:\n1) Draw the damaged area, or enter it in pixels in settings.\n2) Programs and windows are pushed into the safe zone.\n3) For true fullscreen games use 'Lock resolution': the resolution is reduced and centred so the damaged band stays black.\n\nPause: Ctrl+Alt+P\nNote: if a game is not pushed correctly, run BlindSpot as administrator." },
        };

        // Lookups fall back to the key itself, so a missing entry shows up as a
        // wrong-looking string in the UI instead of a crash.
        public static string T(string key)
        {
            var table = Current == En ? EnTable : FaTable;
            return table.TryGetValue(key, out var v) ? v : key;
        }

        // Formatted variant: Lang.T("res_done", w, h). For RTL languages the format
        // string itself is already right-to-left; only the numbers are substituted.
        public static string T(string key, params object[] args)
        {
            try { return string.Format(T(key), args); }
            catch { return T(key); }
        }

        // The whole app reads this; AppConfig.Language sets it at startup and the
        // tray language item flips it at runtime.
        public static string Current { get; set; } = Fa;

        public static bool IsRtl => Current != En;

        // Flips and returns the new code.
        public static string Toggle()
        {
            Current = (Current == En) ? Fa : En;
            return Current;
        }
    }
}
