# Reddit post kit — آماده برای کپی‌پیست

## چگونه استفاده کنی
۱. وارد اکانت شو
۲. یکی از لینک‌های زیر رو باز کن
۳. متن جواب رو کپی کن، یه کلمه تغییر بده (که کپی‌پیست محسوب نشه)
۴. Send

---

## TOP PICK — r/windows (۱۴۰۰ upvote، هنوز فعال)

**لینک:** https://www.reddit.com/r/windows/comments/1apryeo/part_of_my_screen_is_broken_how_do_i_make_windows/

**عنوان تاپیک:** «Part of my screen is broken. How do I make windows just project onto the still intact part of my screen?»

**جوابت:**

> ویندوز خودش یه قابلیت داره که کمتر کسی می‌شناسشش: AppBar API. همون مکانیزمی که تسک‌بار ویندوز باهاش فضاش رو رزرو می‌کنه. اگه یه برنامه به ویندوز بگه «این نوار دیگه وجود نداره»، ویندوز همه چیز رو از اونجا دور می‌چینه — تسک‌بار، آیکن‌ها، پنجره‌های maximized، حتی ماوس نمی‌ره اونجا.
>
> من یه ابزار رایگان ساختم که دقیقاً همین کار رو می‌کنه: https://github.com/NotoriusP/BlindSpot
>
> فقط با ماوس روی قسمت خراب یه مستطیل می‌کشی و تمام. نیازی به تغییر رزولوشن نیست. متن‌باز (MIT) هم هست اگه بخوای خودت دست ببری.

---

## PICK ۲ — r/pchelp (هنوز باز)

**لینک:** https://www.reddit.com/r/pchelp/comments/zjo82a/how_to_not_use_part_of_screen_that_is_broken/

**عنوان تاپیک:** «How to not use part of screen that is broken»

**جوابت:**

> راه حل اینجا AppBar API ویندوزه — همون چیزی که تسک‌بار خودش باهاش جاش رو رزرو می‌کنه. یه برنامه می‌تونه قسمت خراب رو به‌عنوان فضای رزرو‌شده ثبت کنه و ویندوز کاملاً ازش دوری می‌کنه. جوری که انگار اون قسمت فیزیکی قطع شده.
>
> ساختمش: https://github.com/NotoriusP/BlindSpot
>
> رایگان، متن‌باز، نیازی به نصب نداره. فقط مستطیل رو با ماوس رسم می‌کنی.

---

## PICK ۳ — r/ultrawidemasterrace (سوال مستقیم)

**لینک:** https://www.reddit.com/r/ultrawidemasterrace/comments/1awaae7/change_usable_screen_portion_of_broken_monitor/

**عنوان تاپیک:** «Change usable screen portion of broken monitor?» (آیا راهی هست ویندوز رو فریب بدیم فکر کنه صفحه کوچکتره؟)

**جوابت:**

> این دقیقاً کاریه که AppBar API می‌کنه. همون مکانیزمی که تسک‌بار ویندوز برای رزرو کردن جاش استفاده می‌کنه. یه برنامه اون نوار رو به‌عنوان AppBar ثبت می‌کنه (`SHAppBarMessage` با `ABM_NEW`) و ویندوز دیگه هیچی داخلش نمی‌ذاره.
>
> یه ابزار رایگان/mتن‌باز ساختم که همین کار رو می‌کنه — با ماوس روی نوار خراب یه مستطیل بکش و تمام. https://github.com/NotoriusP/BlindSpot
>
> اگه خودت کد می‌زنی، کلیدش همون `SHAppBarMessage` هست.

---

## PICK ۴ — r/Monitors (پرسش مکرر)

**لینک:** https://www.reddit.com/r/Monitors/comments/r3b0v5/can_i_resize_the_screen_i_broke_my_monitor_and_i/

**عنوان تاپیک:** «Can i resize the screen? I broke my monitor and i want to know if there is a way to make the screen smaller and where the screen broke, i want there to be black bars.»

**جوابت:**

> تغییر رزولوشن مشکل اینه که ویندوز تصویر رو stretch می‌کنه یا center می‌کنه، پس باز قسمت‌های خراب رو شامل می‌شه. راه بهتر اینه که به ویندوز بگی اون قسمت کلاً وجود نداره.
>
> ویندوز یه API داره به اسم AppBar — همون چیزی که تسک‌بار باهاش جاش رو رزرو می‌کنه. یه برنامه می‌تونه نوار خراب رو رزرو کنه و ویندوز همه چیز رو بعد از اونجا می‌چینه: تسک‌بار، آیکن‌ها، پنجره‌های maximized.
>
> یه ابزار رایگان ساختم براش: https://github.com/NotoriusP/BlindSpot — فقط با ماوس نوار رو رسم می‌کنی. متن‌باز (MIT) هم هست.

---

## PICK ۵ — r/Monitors (dead zone)

**لینک:** https://www.reddit.com/r/Monitors/comments/9xqepo/dead_zone_on_monitor_image_included/

**عنوان تاپیک:** «Dead zone on monitor (image included)» — نوار مشکی سمت چپ با یه خط آبی.

**جوابت:**

> اگه درایور مانیتور این رو ساپورت نمی‌کنه، خود ویندوز یه راه داره: AppBar API. همون چیزی که تسک‌بار باهاش جاش رو رزرو می‌کنه. یه برنامه می‌تونه اون نوار رو رزرو کنه و ویندوز دیگه موس، آیکن یا پنجره رو اونجا نمی‌ذاره.
>
> ساختمش: https://github.com/NotoriusP/BlindSpot. رایگان، متن‌باز، بدون نصب. با ماوس نوار رو رسم می‌کنی و تمام.

---

## PICK ۶ — r/techsupport (بزرگ‌ترین بازدید)

**لینک:** https://www.reddit.com/r/techsupport/comments/u4ycq4/is_there_a_way_to_use_only_a_portion_of_my_screen/

**عنوان تاپیک:** «Is there a way to use only a portion of my screen?» (۲۵٪ سمت راست خراب)

**جوابت:**

> راه حل اینجا دو تا هست:
>
> **۱. رزولوشن + center:** یه رزولوشن پایین‌تر بگیر و توی GPU settings بگو center کنه به‌جای stretch. ولی رزولوشن اصلی رو از دست می‌دی و نوار سیاه هر دو طرف میاد.
>
> **۲. AppBar (بهتر):** همون API‌ای که تسک‌بار ویندوز استفاده می‌کنه. یه برنامه نوار خراب رو رزرو می‌کنه و ویندوز دیگه ازش استفاده نمی‌کنه — بدون تغییر رزولوشن. من [BlindSpot](https://github.com/NotoriusP/BlindSpot) رو برای همین ساختم: رایگان، متن‌باز، نیازی به نصب نداره.

---

## هشدار مهم

- **هرگز همه‌جا یه متن رو کپی نکن** — Reddit spam detection این رو می‌گیره. **حداقل ۲-۳ کلمه در هر جواب تغییر کنه**
- **اول ۲-۳ روز کارما بگیر** قبل از اینکه لینک بذاری
- **اول از PICK ۳ شروع کن** (ultrawidemasterrace) — چون سوال مستقیم‌ترینه و بازترین ساب‌ریدیت
- r/techsupport ممکنه پست رو حذف کنه (self-promotion) — آخرین انتخاب

---

## برای استراحت کردن اکانت (روزهای اول)

به جای لینک دادن، می‌تونی فقط کمک کنی:

> من یه کار مشابه داشتم. مشکل اینه که تغییر رزولوشن تصویر رو stretch می‌کنه. اگه GPU settings داری، بگو center کنه به جای stretch — یه نوار سیاه هر دو طرف میاد ولی حداقر قسمت خراب رو شامل نمیشه.

این جواب‌ها carma می‌سازن و بعداً اگه کسی ازت بپرسه «چطوری حل کردی؟» می‌تونی لینک بدی.
