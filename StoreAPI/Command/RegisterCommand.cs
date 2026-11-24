using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using StoreAPI.Controllers;

namespace StoreAPI.Command
{

    public class RegisterCommand : IRequest<IdentityResult>
    {
        public string Username { get; }
        public string Email { get; }
        public string Password { get; }
        public string CaptchaAnswer { get; } 
        public string CaptchaToken { get; }  

        public RegisterCommand(string username, string email, string password, string captchaAnswer, string captchaToken)
        {
            Username = username;
            Email = email;
            Password = password;
            CaptchaAnswer = captchaAnswer;
            CaptchaToken = captchaToken;
        }
    }

    //public record RegisterCommand(string Username, string Email, string Password) : IRequest<IdentityResult>;


    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            //  قوانین نام کاربری 

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است.")
                .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد.")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")
                .Matches(@"^[a-zA-Z0-9@._-]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف، اعداد و کاراکترهای @ . _ - باشد.");

            //  قوانین ایمیل 

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .EmailAddress().WithMessage("فرمت ایمیل صحیح نیست.")
                .MaximumLength(100).WithMessage("ایمیل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            //  قوانین پسورد (Password) 

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("پسورد الزامی است.")
                .MinimumLength(6).WithMessage("پسورد باید حداقل ۶ کاراکتر باشد.")
                .MaximumLength(100).WithMessage("پسورد نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.")
                // حتماً باید یک حرف بزرگ داشته باشه
                .Matches("[A-Z]").WithMessage("پسورد باید حداقل یک حرف بزرگ داشته باشد.")
                // حتماً باید یک حرف کوچک داشته باشه
                .Matches("[a-z]").WithMessage("پسورد باید حداقل یک حرف کوچک داشته باشد.")
                // حتماً باید یک عدد داشته باشه
                .Matches("[0-9]").WithMessage("پسورد باید حداقل یک عدد داشته باشد.")
                // حتماً باید یک کاراکتر خاص (مثل !@#$%^&*) داشته باشه
                .Matches("[^a-zA-Z0-9]").WithMessage("پسورد باید حداقل یک کاراکتر خاص (مثل !@#$%^&*) داشته باشد.");
        }
    }


    public class RegisterCommandHandler (UserManager<IdentityUser> _userManager) : IRequestHandler<RegisterCommand, IdentityResult>
    {
        public async Task<IdentityResult> Handle(RegisterCommand request, CancellationToken cancellationToken) 
        {
            try
            {
                var expectedAnswer = CaptchaController.Decrypt(request.CaptchaToken, "MySuperSecretCaptchaKey123!");
                if (expectedAnswer != request.CaptchaAnswer)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "پاسخ کپچا اشتباه است." });
                }
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "توکن کپچا نامعتبر است." });
            }

            if ( await _userManager.FindByNameAsync(request.Username)!=null)
                return IdentityResult.Failed(new IdentityError { Description = "نام کاربری قبلاً ثبت شده است" });


            if (await _userManager.FindByEmailAsync(request.Email)!=null)

                return IdentityResult.Failed(new IdentityError {Description = "ایمیل قبلاً ثبت شده است"});
            

            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                //اضافه کردن نقش
                await _userManager.AddToRoleAsync(user, "Customer"); 
            }
            return result;
        }

    }
}
