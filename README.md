# BlindSpot

<p align="center">
  <img src="UI/Asset/Logo.png" alt="BlindSpot" width="256">
</p>

**ناحیهٔ امن برای مانیتور آسیب‌دیده — A safe zone for a damaged monitor**

<div dir="rtl">

وقتی یه نوار از صفحهٔ مانیتورت سوخته باشه (مثلاً ۱۶۰ پیکسل از لبهٔ چپ دیگه نور
نمی‌ده)، ویندوز همچنان برنامه‌ها و آیکون‌ها رو اونجا می‌چینه. BlindSpot اون نوار
رو می‌گیره و می‌گه «اینجا تعلق به کسی نداره».

## چه می‌کنه؟

۱. **نوار سیاه + رزرو فضا** — نوار آسیب‌دیده رو سیاه می‌کنه و با API مخفیِ AppBar به
ویندوز می‌قبونونده که اون نوار دیگه وجود نداره. آیکون‌های دسکتاپ، تسک‌بار و پنجره‌های
به‌حداکثر‌رسیده خودشون رو سمتِ راستِ نوار چیده می‌کنن. (این همون مکانیزمیه که
DesktopCoral استفاده می‌کرد.)

۲. **هل دادن پنجره‌ها** — هر پنجره‌ای که سرِ نوار قرار بگیره به ناحیهٔ امن هل داده
می‌شه. بازی‌های Borderless-fullscreen هم درست کلایمپ می‌شن. یه میان‌بر Ctrl+Alt+P هم
وجود داره برای توقف موقت (مثلاً وقتی یه بازی به‌درستی جابجا نمی‌شه).

۳. **قفل رزولوشن برای بازی‌های Fullscreen حقیقی** — بازی‌های exclusive-fullscreen از
دستِ هیچ برنامهٔ کاربری کاری نمی‌شن. BlindSpot رزولوشن رو کم می‌کنه (مثلاً
۱۶۰۰x۹۰۰) و وسط صفحه مرکز می‌کنه، تا نوار چپ سیاه بمونه. بعد از بازی، رزولوشن اصلی
بازیابی می‌شه.

## نصب

**نیازی به نصب نیست.** فایل `BlindSpot.exe` رو از [Releases](../../releases) دانلود
کن و اجرا کن. برنامه خودش رو توی system tray قرار می‌ده و از اونجا کار می‌کنه.

## استفاده

۱. روی آیکون BlindSpot توی tray راست‌کلیک کن.
۲. **«رسم ناحیه با ماوس...»** رو بزن و مستطیکی روی نوار آسیب‌دیده بکش. (یا
**«تنظیمات ناحیهٔ امن...»** و وارد کردن پیکسل.)
۳. از این به بعد ویندوز از اون نوار دوری می‌کنه.

**زبان:** از منوی راست‌کلیک، «زبان: English» رو بزن تا انگلیسی/فارسی переключ بشه.

**توقف موقت:** Ctrl+Alt+P (مثلاً وقتی یه بازی گیر می‌کنه).

## note

اگه یه بازی به‌درستی هل داده نمی‌شه، BlindSpot رو به‌صورت مدیر (Administrator) اجرا
کن. برنامه آیکونش رو توی tray می‌ذاره و خودش رو مخفی می‌کنه — هیچ پنجره‌ای باز
نمی‌مونه.

## محدودیت‌ها

- **Exclusive-fullscreen واقعی:** دسترسیِ user-mode بهش وجود نداره؛ به‌جاش از قفل
  رزولوشن استفاده می‌شه. توی بازی، رزولوشن رو همون مقدار قفل‌شده بذار (یا حالت
  Borderless انتخاب کن).
- **چند مانیتور:** فقط مانیتور اصلی (PrimaryScreen) رو می‌گیره. ترکیب‌کردن با
  مانیتور سالم نیاز به کارِ بیشتر داره.
- ** DPI:** روی PerMonitorV2 کار می‌کنه. اگه سیستم جیزیزن، آیکون و نوار هم‌تراز
  نمی‌شن.

## تحت‌الرقابة (for nerds)

| فایل | کار |
|---|---|
| `Core/AppBarBand.cs` | ثبت نوار به‌عنوان AppBar با `SHAppBarMessage` — هم پوشش سیاه، هم رزرو فضا، تو یه پنجره |
| `Core/WindowClamper.cs` | اسکن EnumWindows و هل دادن پنجره‌ها؛ timeout برای جلوگیری از تقلا |
| `Core/ResolutionManager.cs` | enumerate modes، انتخاب، مرکزسازی با `DMDFO_CENTER` |
| `Core/ZoneMath.cs` | هندسهٔ حاشیه‌ها + clamp (حداکثر ۴۸۰px) |
| `Core/Lang.cs` | دیکشنری دوزبانهٔ fa/en |
| `UI/TrayForm.cs` | tray، منو، مدیریت همه subsystemها |

پروژه با .NET 8 WinForms ساخته شده. `dotnet build -c Release` برای بیلد معمولی،
`publish.ps1` برای خروجیِ تک‌فایل.

</div>

---

<div dir="ltr">

When a strip of your monitor is dead (say the leftmost 160px no longer lights up),
Windows still places icons and windows there. BlindSpot claims that strip and says
"this area belongs to nobody".

## What it does

1. **Black band + space reservation** — blackens the damaged strip and, through the
   undocumented AppBar API, convinces Windows that the strip does not exist. Desktop
   icons, the taskbar and maximised windows arrange themselves to the right of it.
   (Same mechanism DesktopCoral used.)

2. **Window pushing** — any window sitting on the strip gets nudged into the safe
   zone. Borderless-fullscreen games clamp correctly too. A Ctrl+Alt+P hotkey pauses
   it (e.g. when a game misbehaves).

3. **Resolution lock for true fullscreen games** — exclusive-fullscreen games cannot
   be moved by any user-mode program. BlindSpot lowers the resolution (e.g.
   1600x900) and centres it, so the left band stays black. Original resolution is
   restored afterwards.

## Install

**No install required.** Download `BlindSpot.exe` from
[Releases](../../releases) and run it. It lives in the system tray.

## Usage

1. Right-click the BlindSpot tray icon.
2. Pick **"Draw zone with mouse..."** and drag a rectangle over the damaged strip.
   (Or **"Safe zone settings..."** to type pixels.)
3. Windows avoids that strip from now on.

**Language:** from the right-click menu, click "Language: فارسی" to toggle
English/Persian.

**Pause:** Ctrl+Alt+P (e.g. when a game gets stuck).

## Note

If a game is not pushed correctly, run BlindSpot as Administrator. The app puts its
icon in the tray and hides itself — no window stays open.

## Limitations

- **True exclusive-fullscreen** is unreachable from user mode; the resolution lock is
  the workaround. Set the in-game resolution to the locked value (or pick Borderless).
- **Multiple monitors:** only the primary screen is handled.
- **DPI:** works under PerMonitorV2. On a misconfigured system the icon and band may
  not align.

## Building

Requires .NET 8 SDK. `dotnet build -c Release` for a normal build; `publish.ps1`
for a single-file self-contained exe (no .NET install needed on the target machine).

</div>
