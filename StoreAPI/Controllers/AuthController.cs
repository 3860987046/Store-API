using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StoreAPI.DTOs;
using StoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using StoreAPI.Features.Auth.Commands;


namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly IMediator _mediator; 

        public AuthController(UserManager<IdentityUser> userManager , JwtService jwtService , IMediator mediator)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _mediator = mediator;
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

            //  گارسون با استفاده از ابزار mediator برگه سفارش رو مطابق سازنده پر میکنه و میندازه توی صندوق 
            var result = await _mediator.Send(new RegisterCommand(registerDto.Username, registerDto.Email, registerDto.Password));

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
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //'گارسون یا ابزار mediatorیک برگه سفارش لاگین جدید مطابق سازنده پر میکنه و اون رو sendمیکنه
            var token = await _mediator.Send(new LoginCommand(loginDto.Username, loginDto.Password));  
            // 4. اگر پسورد اشتباه بود، باز هم همان پیام عمومی را می‌دهیم (برای امنیت)
            if (token==null)
                return Unauthorized(new { Message = "نام کاربری یا پسورد اشتباه است" });


            // برای برگردوندن UserId، دوباره کاربر رو پیدا میکنیم (چون Command فقط توکن رو برگردوند)
            var user = await _userManager.FindByNameAsync(loginDto.Username);


            // 6. برگرداندن توکن به همراه اطلاعات پایه کاربر
            return Ok(new
            {
                Message = "ورود موفقیت‌آمیز بود",
                Token = token,
                UserId = user?.Id,
                Username = user?.UserName
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
