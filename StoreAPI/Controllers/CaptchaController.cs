using Microsoft.AspNetCore.Mvc;
using SkiaSharp; //  کتابخانه‌ای برای کشیدن تصویر (مثل نقاشی)
using System.Security.Cryptography; //  برای رمزنگاری (قفل کردن) جواب کپچا
using System.Text;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaptchaController : ControllerBase
    {
        // این یک کلید مخفی است که فقط سرور می‌دونه. (مثل کلید گاوصندوق)
        private const string SecretKey = "MySuperSecretCaptchaKey123!";

        [HttpGet("image")]
        public IActionResult GetCaptchaImage()
        {
            // 1. یک رشته تصادفی (مثل A3B9X1) می‌سازیم.
            var randomString = GenerateRandomString(6);

            // 2. این رشته رو با کلید مخفی قفل می‌کنیم. (به این می‌گن توکن)
            var token = Encrypt(randomString, SecretKey);

            // 3. یک بوم نقاشی خالی با ابعاد ۱۵۰ در ۵۰ پیکسل درست می‌کنیم.
            using var bitmap = new SKBitmap(150, 50);
            using var canvas = new SKCanvas(bitmap);

            // 4. کل بوم رو با رنگ خاکستری روشن پر می‌کنیم.
            canvas.Clear(SKColors.LightGray);

            // 5. یک قلم‌مو (Font) برای نوشتن حروف آماده می‌کنیم.
            // (اسم فونت Arial، سایز ۳۵)
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 35);

            // 6. یک رنگ برای نوشتن (آبی تیره) انتخاب می‌کنیم.
            using var paint = new SKPaint
            {
                Color = SKColors.DarkBlue,
                IsAntialias = true //  لبه‌های حروف صاف و نرم باشن
            };

            var random = new Random(); // برای چرخش تصادفی حروف

            // 7. حالا تک‌تک حروف رو روی بوم می‌کشیم.
            for (int i = 0; i < randomString.Length; i++)
            {
                // زاویه چرخش تصادفی (مثلاً بین -۲۰ تا +۲۰ درجه)
                var angle = random.Next(-20, 20);

                canvas.Save(); // وضعیت بوم رو ذخیره می‌کنیم
                canvas.RotateDegrees(angle, 20 + (i * 20), 30); //  حرف رو کج می‌کنیم

                //  حرف رو روی بوم می‌نویسیم
                canvas.DrawText(randomString[i].ToString(), 20 + (i * 20), 35, SKTextAlign.Left, font, paint);

                canvas.Restore(); // به حالت قبل برمی‌گردیم
            }

            // 8. چند تا خط خط‌خطی (نویز) می‌کشیم تا خوندن کپچا سخت‌تر بشه.
            using var noisePaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 2 };
            for (int i = 0; i < 5; i++)
            {
                canvas.DrawLine(random.Next(150), random.Next(50), random.Next(150), random.Next(50), noisePaint);
            }

            // 9. بوم نقاشی رو تبدیل به یک عکس PNG می‌کنیم.
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var imageBytes = data.ToArray();

            // 10. توکن (جواب قفل شده) رو توی هدر پاسخ می‌ذاریم تا کلاینت اون رو بگیره.
            Response.Headers.Append("X-Captcha-Token", token);

            // 11. عکس نهایی رو به کلاینت برمی‌گردونیم.
            return File(imageBytes, "image/png");
        }

        // این متد یک رشته تصادفی (مثل A3B9X1) می‌سازه.
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // این متد متن رو با کلید مخفی قفل می‌کنه.
        private string Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs)) { sw.Write(plainText); }
            return Convert.ToBase64String(ms.ToArray());
        }

        // این متد متن قفل شده رو باز می‌کنه (برای اعتبارسنجی در Register).
        public static string Decrypt(string cipherText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}