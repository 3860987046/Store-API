using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using StoreAPI.Command.Auths;
using StoreAPI.Query.Auths;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController( IMediator _mediator) : ControllerBase
    {

        // POST: api/Auth/register

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // اعتبارسنجی اولیه فرم (مثلاً پسورد خالی نباشد)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //شما در حال تبدیل کردن داده‌های لایه وب (DTO) به داده‌های لایه منطق (Command) هستید.

            var result = await _mediator.Send(new RegisterCommand(registerDto.Username, registerDto.Email,
                registerDto.Password, registerDto.CaptchaAnswer, registerDto.CaptchaToken));

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "کاربر با موفقیت ثبت‌نام شد" });
        }



        // POST: api/Auth/login

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //'گارسون یا ابزار mediatorیک برگه سفارش لاگین جدید مطابق سازنده پر میکنه و اون رو sendمیکنه و منتظر هست که loginResponseدریافت کند
            var loginResponse = await _mediator.Send(new LoginCommand(loginDto.Username, loginDto.Password));
            // 4. اگر پسورد اشتباه بود، باز هم همان پیام عمومی را می‌دهیم (برای امنیت)
            if (loginResponse == null)
                return Unauthorized(new { Message = "نام کاربری یا پسورد اشتباه است" });

            return Ok(loginResponse);
        }

        // Get: api/Auth/me

        [HttpGet("me")]
        [Authorize] // فقط کاربرانی که توکن معتبر دارند می‌توانند این متد را صدا بزنند
        public async Task<IActionResult> GetCurrentUser()
        {
            //****************** استخراج نام کاربری از توکن در لایه کنترلر*****************
            //در هدر درخواست authorize، یک httpcontex.user ایجاد میشود که همان claimprinciple  است
            //حال این claimprinciple ممکن است صفر، یک یا چندین identifier داشته باشد
            //که ما از اولین identifier نوع claimTypes.nameش رو دریافت میکنیم
            //توجه شود اگر احراز هویت درست نباشد، میدل ور app.useAuthentication
            //این claimprinciple راپرنمیکند ومقدار identifier برابر null است

            //var username = User.FindFirstValue(ClaimTypes.Name);
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { Message = "توکن نامعتبر است" });

            // استخراج username و ارسال به Query
            var profile = await _mediator.Send(new GetCurrentMeQuery(username));

            if (profile is null)
                return NotFound(new { Message = "کاربری یافت نشد" });

            return Ok(profile);
        }


        // GET: api/Auth/{id}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            // ارسال به Query
            var profile = await _mediator.Send(new GetUserByIdQuery(id));

            if (profile is null)
                return NotFound(new { Message = "کاربری با این شناسه یافت نشد" });

            return Ok(profile);
        }

    }
}
