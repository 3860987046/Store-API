using Microsoft.IdentityModel.Tokens;          // کلاس‌های رمزنگاری: SymmetricSecurityKey، SigningCredentials، SecurityAlgorithms
using Microsoft.AspNetCore.Identity; // برای استفاده از IdentityUser
using StoreAPI.Models;                          // مدل‌های پروژه، از جمله کلاس User
using System.IdentityModel.Tokens.Jwt;          // کلاس‌های ساخت و نوشتن توکن JWT: JwtSecurityToken، JwtSecurityTokenHandler
using System.Security.Claims;                   // کلاس Claim و ClaimTypes برای تعریف اطلاعات داخل توکن
using System.Text;                              // Encoding برای تبدیل رشته به بایت (UTF8)


namespace StoreAPI.Services
{


    // مسئولیت: ساخت و امضای توکن JWT برای یک کاربر مشخص
    public class JwtService
    {
    
        // از این طریق به مقادیر Jwt:Key، Jwt:Issuer و ... دسترسی داریم
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;  
        }

        // هدف: دریافت یک User و برگرداندن یک رشته‌ی توکن JWT امضاشده
        // مراحل: ساخت کلید ← ساخت اعتبارنامه امضا ← تعریف claims
        //         ← ساخت توکن ← سریالایز کردن به رشته
        public string GenerateToken(IdentityUser user)
        {      
            // _config["Jwt:Key"] رشته‌ی مخفی را می‌خواند
            // Encoding.UTF8.GetBytes آن را به آرایه بایت تبدیل می‌کند (رمزنگاری روی بایت کار می‌کند)
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            // ساخت اعتبارنامه‌ی امضا: کلید + الگوریتم HMAC-SHA256
            // این شیء تعیین می‌کند توکن چگونه امضا شود (alg = HS256)
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                
                // ToString چون مقدار Claim باید رشته باشد
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  
                //   توجه شود کهuser.Username را به user.UserName تبیدل کردیم چون مربوط به کلاس UserIdentity بود نه کلاس User 
                new Claim(ClaimTypes.Name, user.UserName),  
                // new Claim(ClaimTypes.Role, user.Role)  // اگه Role داری
            };

            // ساخت شیء توکن JWT با تنظیمات کامل
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],           // صادرکننده‌ی توکن (StoreAPI)
                audience: _config["Jwt:Audience"],       // مخاطب توکن (StoreAPIClient)
                claims: claims,                          // اطلاعات کاربر
                expires: DateTime.UtcNow.AddMinutes(     // زمان انقضا: اکنون + مدت اعتبار
                                                         // مقدار DurationInMinutes از تنظیمات رشته است، به double تبدیل می‌شود
                                                         // تا AddMinutes بتواند از آن استفاده کند
                    double.Parse(_config["Jwt:DurationInMinutes"])),
                signingCredentials: credentials          // امضای توکن با کلید و الگوریتم بالا
            );

            // تبدیل شیء توکن به رشته‌ی نهایی JWT (سریالایز کردن)
            // خروجی همان رشته‌ای است که به کلاینت داده می‌شود (xxxxx.yyyyy.zzzzz)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}