
using MediatR;

namespace StoreAPI.Features.Auth.Commands
{
    //  برگه سفارش لاگین. خروجی: یک رشته (توکن JWT) یا null (اگر لاگین ناموفق بود)

    public class LoginCommand: IRequest<string?>
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginCommand(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
