using MediatR;
using Microsoft.AspNetCore.Identity;

namespace StoreAPI.Features.Auth.Commands
{
    // 👨‍🍳 این کلاس آشپزه. قرارداد IRequestHandler یعنی: «من مسئول پختن RegisterCommand هستم و خروجیم IdentityResult است.»
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, IdentityResult>
    {
        private readonly UserManager<IdentityUser> _userManager; //  ابزار آشپز 

        // تزریق وابستگی (Dependency Injection): به آشپز میگیم ابزارت رو از اینجا بگیر
        public RegisterCommandHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        // متد Handle: وقتی MediatR برگه سفارش رو به این آشپز داد، این متد اجرا میشه
        public async Task<IdentityResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. چک میکنیم نام کاربری تکراری نباشه
            if (await _userManager.FindByNameAsync(request.Username) != null)
                return IdentityResult.Failed(new IdentityError { Description = "نام کاربری قبلاً ثبت شده است" });

            // 2. چک میکنیم ایمیل تکراری نباشه
            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return IdentityResult.Failed(new IdentityError { Description = "ایمیل قبلاً ثبت شده است" });

            // 3. یک شیء کاربر جدید می‌سازیم (اطلاعات پایه)
            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };


            // 4. به UserManager میگیم این کاربر رو با این پسورد توی دیتابیس ذخیره کن
            return await _userManager.CreateAsync(user, request.Password);
        }

    }

}
