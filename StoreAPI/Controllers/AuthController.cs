using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StoreAPI.DTOs;
using StoreAPI.Services;
using Microsoft.AspNetCore.Authorization;


namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtService _jwtService;

        public AuthController(UserManager<IdentityUser> userManager , JwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }


        // POST: api/Auth/register
        // ==========================================
        // 1. ثبت‌نام (Register)
        // ==========================================

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // اعتبارسنجی اولیه فرم (مثلاً پسورد خالی نباشد)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // بررسی اینکه آیا نام کاربری قبلاً وجود دارد یا نه
            var userExists = await _userManager.FindByNameAsync(registerDto.Username) != null;
            if (userExists)
                return BadRequest(new { Message = "نام کاربری قبلاً ثبت شده است" });

            // بررسی اینکه آیا ایمیل قبلاً وجود دارد یا نه
            var emailExists = await _userManager.FindByEmailAsync(registerDto.Email) != null;
            if (emailExists)
                return BadRequest(new { Message = "ایمیل قبلاً ثبت شده است" });

            // ساخت یک شیء از کاربر (فقط اطلاعات پایه، پسورد را UserManager خودش هش می‌کند)
            var user = new IdentityUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email
            };
            // ذخیره کاربر در دیتابیس با پسورد (UserManager پسورد را هش می‌کند و ذخیره می‌کند)
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // اگر خطایی رخ داد (مثلاً پسورد ضعیف بود)، لیست خطاها را برمی‌گردانیم
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "کاربر با موفقیت ثبت‌نام شد" });
        }
        // ==========================================


        // POST: api/Auth/login
        // ==========================================
        // 2. لاگین (Login) و دریافت توکن JWT
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. پیدا کردن کاربر با نام کاربری
            var user = await _userManager.FindByNameAsync(model.Username);

            // 2. اگر کاربر وجود نداشت، پیام خطای عمومی می‌دهیم (نمی‌گوییم "نام کاربری اشتباه است")
            if (user == null)
                return Unauthorized(new { Message = "نام کاربری یا پسورد اشتباه است" });

            // 3. چک کردن پسورد با استفاده از UserManager (مقایسه پسورد خام با هش ذخیره شده)
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            // 4. اگر پسورد اشتباه بود، باز هم همان پیام عمومی را می‌دهیم (برای امنیت)
            if (!isPasswordValid)
                return Unauthorized(new { Message = "نام کاربری یا پسورد اشتباه است" });

            // 5. تولید توکن JWT برای این کاربر
            var token = _jwtService.GenerateToken(user);

            // 6. برگرداندن توکن به همراه اطلاعات پایه کاربر
            return Ok(new
            {
                Message = "ورود موفقیت‌آمیز بود",
                Token = token,
                UserId = user.Id,
                Username = user.UserName
            });
        }

        // Get: api/Auth/me
        // ==========================================
        // 3. گرفتن اطلاعات کاربر جاری 
        // ==========================================
        [HttpGet("me")]
        [Authorize] // فقط کاربرانی که توکن معتبر دارند می‌توانند این متد را صدا بزنند
        public async Task<IActionResult> GetCurrentUser()
        {
            // استخراج نام کاربری از توکن JWT (چون در JwtService آن را ClaimTypes.Name کردیم)
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { Message = "توکن نامعتبر است" });

            // پیدا کردن کاربر در دیتابیس
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { Message = "کاربری یافت نشد" });

            return Ok(new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email
            });
        }

        // GET: api/Auth/{id}
        // ==========================================
        // 4. گرفتن اطلاعات کاربر با ID 
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            // پیدا کردن کاربر با شناسه (Id)
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "کاربری با این شناسه یافت نشد" });

            return Ok(new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email
            });
        }
    }
}
