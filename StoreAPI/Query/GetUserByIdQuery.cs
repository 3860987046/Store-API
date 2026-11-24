using MediatR;
using Microsoft.AspNetCore.Identity;

namespace StoreAPI.Query
{
    public class GetUserByIdResponse
    {
        //من حتی اگه خروجی م null باشه به تو یه گزارش خالی میدم
        public string Id { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public class GetUserByIdQuery : IRequest<GetUserByIdResponse?>
    {
        //  انتقال از لایه وب  به لایه کنترلر
        public string Id { get; }
        public GetUserByIdQuery(string id)
        {
            Id = id;
        }
    }


    public class GetUserByIdQueryHandler (UserManager<IdentityUser> _userManager) : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse?>
    {

        public async Task<GetUserByIdResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user is null)
                return null;

            return new GetUserByIdResponse
            {
                //اگر id وارد نشود یا در دیتابیس نباشد ما اصلا به سراغ Responseنمیرویم 
                //اما اگه درست باشد، ممکن است کاربر Email ش را در دیتابیس ثبت نکرده باشد که باید خالی بفرستیم
                Id = user.Id ,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };
        }
    }
}
