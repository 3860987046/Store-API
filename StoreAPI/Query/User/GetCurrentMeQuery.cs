using MediatR;
using Microsoft.AspNetCore.Identity;

namespace StoreAPI.Query.User
{
    public class GetCurrentMeResponse
    {
        public string Id { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public class GetCurrentMeQuery : IRequest<GetCurrentMeResponse?>
    {
        public string Username { get; }
        public GetCurrentMeQuery(string username)
        {
            Username = username;
        }
    }

    public class GetUserMeQueryHandler(UserManager<IdentityUser> _userManager) : IRequestHandler<GetCurrentMeQuery, GetCurrentMeResponse?>
    {

        public async Task<GetCurrentMeResponse?> Handle(GetCurrentMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Username);
            if (user is null)
                return null;

            return new GetCurrentMeResponse
            {
                //یعنی کاربر مجوز داشته که به مرحله دریافت response رسیده اما ممکن است نام کاربری و ایمیلش در دیتابیس نباشد
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
            };
        }
    }
}


