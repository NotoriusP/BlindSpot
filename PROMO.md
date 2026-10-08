# Promotion kit — کپی‌پیست کن

فقط متنی که نیاز داری رو کپی کن. لینک: https://github.com/NotoriusP/BlindSpot
ریلیز مستقیم: https://github.com/NotoriusP/BlindSpot/releases/latest

---

## Reddit — r/laptops / r/techsupport (انگلیسی، کوتاه)

**Title:**
> My laptop screen's left edge is dead — I wrote a tool that hides that strip from Windows entirely

**Body:**
> The leftmost 160px of my panel is burned out. Windows kept putting icons, the taskbar and maximised windows under it, so I looked for a way to just... tell Windows that region doesn't exist.
>
> Turns out the AppBar API (`SHAppBarMessage`) does exactly that — it's how the taskbar reserves its own space. So I built a small tray app around it:
>
> - drag a rectangle over the damaged area, it registers it as an AppBar band
> - the desktop, taskbar and maximised windows never render inside it
> - fullscreen games get a resolution lock with centring instead (exclusive-fullscreen can't be moved from user mode)
>
> Result: a usable laptop with a 160px black bar instead of a unusable one. Free, MIT, single-file exe, no install.
>
> https://github.com/NotoriusP/BlindSpot

---

## Reddit — r/laptops (کوتاه‌تر، برای نظرسنجی)

> The left edge of my laptop screen died (160px, no backlight). Instead of replacing the panel I made BlindSpot — a tray app that registers that strip as a Windows AppBar so nothing ever renders there. Taskbar, icons and maximised windows all start after the band. Source + exe: https://github.com/NotoriusP/BlindSpot
>
> Anyone else dealt with a half-dead panel? Curious what your workaround was.

---

## AlternativeTo — فیلدهای فرم

**App name:** BlindSpot
**Category:** System & Hardware / Desktop Customization
**Official website:** https://github.com/NotoriusP/BlindSpot
**Short description:**
> Hides a dead or damaged strip of your monitor from Windows by registering it as an AppBar band, so the taskbar, desktop icons and maximised windows never render there. Includes a resolution lock with centring for exclusive-fullscreen games.
**Tags:** screen, monitor, desktop, damaged-screen, appbar, windows, tray

---

## تلگرام / توییتر فارسی (کوتاه)

> لبهٔ مانیتور لپ‌تاپم سوخته بود و ویندوز هنوز آیکون‌ها و پنجره‌ها رو اونجا می‌ذاشت.
> BlindSpot رو ساختم: با AppBar API ویندوز اون نوار رو «رزرو» می‌کنه و دیگه هیچی داخلش رندر نمی‌شه.
> رایگان، متن‌باز (MIT)، بدون نصب — فقط exe رو اجرا کن.
> https://github.com/NotoriusP/BlindSpot

---

## HackerNews — Show HN (در صورت تمایل)

**Title:** Show HN: BlindSpot – Hide a dead monitor strip from Windows via the AppBar API

**Body:**
> The leftmost 160px of my laptop panel no longer lights up. Windows still treats it as usable space, so maximised windows, desktop icons and the taskbar kept disappearing under it.
>
> BlindSpot registers that strip with `SHAppBarMessage` (ABM_NEW/ABM_SETPOS) — the same mechanism the taskbar uses to reserve its own space. Windows then never places anything there. A drag-to-define overlay picks the region; the bands are clamped to 480px / 35% of the screen so a misdrawn rectangle can't black out everything.
>
> Some details worth noting:
>
> - `SPI_SETWORKAREA` doesn't work — Explorer silently reverts it. AppBar is the reliable path.
> - Exclusive-fullscreen games own the scan-out path and can't be moved, so the app lowers the panel resolution and asks the GPU to centre it (`DMDFO_CENTER`) instead. Custom timings (1680x1080) get rejected by the Intel driver with `DISP_CHANGE_BADMODE`, so it picks from the modes the driver actually exposes.
> - Applying the band is idempotent — it compares the desired band against the live one and skips re-registration, which is what stopped the flicker.
>
> MIT, .NET 8, single-file self-contained exe. https://github.com/NotoriusP/BlindSpot

---

## چک‌لیست

- [ ] Reddit: r/laptops
- [ ] Reddit: r/techsupport
- [ ] AlternativeTo ثبت
- [ ] تلگرام فارسی
- [ ] Show HN (اختیاری)
