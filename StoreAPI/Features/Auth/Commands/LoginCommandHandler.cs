using MediatR;
using Microsoft.AspNetCore.Identity;
using StoreAPI.Services;


namespace StoreAPI.Features.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler <LoginCommand, string?>
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtService _jwtService; 

        public LoginCommandHandler(UserManager<IdentityUser> userManager, JwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<string?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. پیدا کردن کاربر با نام کاربری
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return null; 

            // 2. چک کردن پسورد
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return null;

            // 3. اگر همه چیز درست بود، توکن JWT رو تولید و برگردون
            return _jwtService.GenerateToken(user);
        }
    }
}
