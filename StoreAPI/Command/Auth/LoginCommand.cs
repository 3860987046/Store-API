using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using StoreAPI.Services;

namespace StoreAPI.Command.Auth
{
    public class LoginResponse
    {
        //اگر کاربر اطلاعات رو درست وارد کنه این شی ساخته میشه 
        //این کار رو فقط برای کم کردن هشدارهای کامپایلر انجام دادم
        //وگرنه که کاربری که درست لاگین کرده حتما این فیلدها را دارد
        public string Message { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;

    }
    //  برگه سفارش لاگین. خروجی: یک رشته توکن  یا null (اگر لاگین ناموفق بود)
    public class LoginCommand : IRequest<LoginResponse?>
    {
        //این مقادیر یکبار ان هم در لحطه ای که شی ساخته میشه مقدار میگیرند
        // از public و getاستفاده میکنیم  تا کنترلر آن را دست نخورده به متد handler
        // برساند این نکته امنیتی دارد تا بعدا میدلوری وسط راه ان را نتواند تغییر بدهد 
        public string Username { get; }
        public string Password { get; }

        public LoginCommand(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    //public record LoginCommand(string Username, string Password) : IRequest<LoginResponse?>;



    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است.")
                .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد.")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")
                .Matches(@"^[a-zA-Z0-9@._-]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف، اعداد و کاراکترهای @ . _ - باشد.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("پسورد الزامی است.")
                .MinimumLength(6).WithMessage("پسورد باید حداقل ۶ کاراکتر باشد.")
                .MaximumLength(100).WithMessage("پسورد نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");
        }
    }



    public class LoginCommandHandler(UserManager<IdentityUser> _userManager,
        JwtService _jwtService) : IRequestHandler<LoginCommand, LoginResponse?>
    {
        public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return null;

            //  پاس دادن userManager به JwtService
            var token = await _jwtService.GenerateToken(user, _userManager);


            return new LoginResponse
            {
                Message = "ورود موفقیت‌آمیز بود",
                Token = token,
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
            };
        }
    }

}

