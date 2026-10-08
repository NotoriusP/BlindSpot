<p align="center">
  <img src="UI/Asset/Logo.png" alt="BlindSpot" width="256">
</p>

# BlindSpot

**ناحیهٔ امن برای مانیتورهای آسیب‌دیده — A safe zone for a damaged monitor**

<div dir="rtl">

وقتی بخشی از مانیتورت سوخته یا پیکسل‌هایش مرده باشد (مثلاً ۱۶۰ پیکسل از لبهٔ چپ دیگر نور نمی‌دهد)، ویندوز همچنان به روال عادی خود ادامه می‌دهد و پنجره‌ها و آیکون‌ها را در آن قسمتِ معیوب قرار می‌دهد. BlindSpot این نوار آسیب‌دیده را «تصاحب» می‌کند و به ویندوز می‌فهماند که این محدوده دیگر قابل‌استفاده نیست.

## ویژگی‌ها

۱. **ایجاد نوار سیاه + رزرو فضا:** نوار آسیب‌دیده را سیاه کرده و با استفاده از APIهای مخفیِ AppBar، به ویندوز اعلام می‌کند که این فضا رزرو شده است. در نتیجه آیکون‌های دسکتاپ، تسک‌بار و پنجره‌های Maximize شده به‌صورت خودکار در سمت راستِ این نوار قرار می‌گیرند.

۲. **تغییر موقعیت (Clamping) پنجره‌ها:** هر پنجره‌ای که در محدودهٔ آسیب‌دیده قرار بگیرد، به‌صورت خودکار به «ناحیهٔ امن» منتقل (Clamp) می‌شود. بازی‌های Borderless-fullscreen هم به همین صورت مدیریت می‌شوند. یک میان‌بر `Ctrl+Alt+P` برای توقف موقت برنامه در نظر گرفته شده است.

۳. **قفل رزولوشن برای بازی‌های Fullscreen:** بازی‌های Exclusive-fullscreen توسط برنامه‌های User-mode قابل جابجایی نیستند. BlindSpot در این حالت رزولوشن را تغییر می‌دهد (مثلاً ۱۶۰۰x۹۰۰) و آن را در مرکز صفحه قرار می‌دهد تا نوار سیاه در سمت چپ باقی بماند. پس از بستن بازی، رزولوشن اصلی سیستم بازیابی می‌شود.

## نصب و اجرا

**بدون نیاز به نصب.** کافیست فایل `BlindSpot.exe` را از بخش [Releases](../../releases) دانلود و اجرا کنید. برنامه در System Tray قرار گرفته و در پس‌زمینه فعالیت می‌کند.

## روش استفاده

۱. روی آیکون BlindSpot در Tray راست‌کلیک کنید.
۲. گزینهٔ **«Draw zone with mouse...»** را انتخاب کرده و با ماوس محدودهٔ آسیب‌دیده را مستطیل بکشید (یا از **«Safe zone settings...»** مختصات دقیق پیکسل‌ها را وارد کنید).
۳. از این لحظه، ویندوز فضای رزرو شده را نادیده می‌گیرد.

*   **تغییر زبان:** از منوی راست‌کلیک، گزینهٔ «Language» را برای تغییر بین فارسی و انگلیسی انتخاب کنید.
*   **نکته:** اگر بازی‌ها به‌درستی جابجا نمی‌شوند، برنامه را با دسترسی **Administrator** اجرا کنید.

## محدودیت‌ها

- **Exclusive-fullscreen:** به‌دلیل محدودیت‌های سیستم‌عامل، تنها راهکار در بازی‌های Fullscreen استفاده از قفل رزولوشن است. توصیه می‌شود در بازی‌ها از حالت Borderless استفاده کنید.
- **چند مانیتوری:** این ابزار در حال حاضر فقط مانیتور اصلی (Primary) را مدیریت می‌کند.
- **DPI:** برنامه با PerMonitorV2 سازگار است، اما در سیستم‌های با تنظیمات DPI غیرمتعارف، ممکن است نوارها به‌درستی تراز نشوند.

## زیرِ کاپوت (جزئیات فنی)

| فایل | وظیفه |
|---|---|
| `Core/AppBarBand.cs` | ثبت نوار به‌عنوان AppBar با استفاده از `SHAppBarMessage` برای رزرو فضا |
| `Core/WindowClamper.cs` | اسکن پنجره‌ها (`EnumWindows`) و انتقال آن‌ها به محدودهٔ امن |
| `Core/ResolutionManager.cs` | مدیریت لیست رزولوشن‌ها و تغییر حالت صفحه با `DMDFO_CENTER` |
| `Core/ZoneMath.cs` | محاسبات حاشیه‌ها و محدودسازی (حداکثر ۴۸۰ پیکسل) |
| `UI/TrayForm.cs` | مدیریت Tray، منوها و هماهنگی زیرسیستم‌ها |

پروژه با **.NET 8 WinForms** توسعه یافته است. از `dotnet build -c Release` برای بیلد معمولی و `publish.ps1` برای خروجی تک‌فایل استفاده کنید.

</div>

---

<div dir="ltr">

When a strip of your monitor is dead (say the leftmost 160px no longer lights up), Windows still places icons and windows there. BlindSpot claims that strip and says "this area belongs to nobody".

## What it does

1. **Black band + space reservation** — blackens the damaged strip and, through the undocumented AppBar API, convinces Windows that the strip does not exist. Desktop icons, the taskbar and maximized windows arrange themselves to the right of it.

2. **Window pushing** — any window sitting on the strip gets nudged into the safe zone. Borderless-fullscreen games clamp correctly too. A `Ctrl+Alt+P` hotkey pauses it (e.g. when a game misbehaves).

3. **Resolution lock for true fullscreen games** — exclusive-fullscreen games cannot be moved by any user-mode program. BlindSpot lowers the resolution (e.g. 1600x900) and centers it, so the left band stays black. Original resolution is restored afterwards.

## Install

**No install required.** Download `BlindSpot.exe` from [Releases](../../releases) and run it. It lives in the system tray.

## Usage

1. Right-click the BlindSpot tray icon.
2. Pick **"Draw zone with mouse..."** and drag a rectangle over the damaged strip (or use **"Safe zone settings..."** to input exact pixels).
3. Windows avoids that strip from now on.

*   **Language:** Switch between English/Persian via the tray menu.
*   **Note:** If a game is not pushed correctly, run BlindSpot as Administrator.

## Limitations

- **Exclusive-fullscreen:** True exclusive-fullscreen is unreachable from user mode; the resolution lock is the workaround. Borderless mode is recommended.
- **Multiple monitors:** Only the primary screen is handled.
- **DPI:** Works under PerMonitorV2.

## Under the hood

| File | Responsibility |
|---|---|
| `Core/AppBarBand.cs` | Registering the band as an AppBar via `SHAppBarMessage` |
| `Core/WindowClamper.cs` | Scanning `EnumWindows` and nudging windows into the safe zone |
| `Core/ResolutionManager.cs` | Enumerating modes and centering with `DMDFO_CENTER` |
| `Core/ZoneMath.cs` | Geometry calculations and clamping logic |
| `UI/TrayForm.cs` | Tray management, menus, and subsystem orchestration |

Built with **.NET 8 WinForms**. Use `dotnet build -c Release` for normal builds or `publish.ps1` for a single-file executable.

</div>
