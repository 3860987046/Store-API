using MediatR;
using Microsoft.AspNetCore.Identity;


namespace StoreAPI.Features.Auth.Commands
{
     // این کلاس برگه سفارشه. می‌گه: «من می‌خوام یک کاربر جدید ثبت‌نام کنم.»
    // خروجی من از نوع IdentityResult است (چون UserManager این نوع رو برمی‌گردونه)
    public class RegisterCommand:IRequest<IdentityResult>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public RegisterCommand(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }
    }
}
