using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;

namespace StoreAPI.Query
{
    public class CommentApprovedResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class GetApprovedCommentsQuery : IRequest<List<CommentApprovedResponse>>
    {
        public int ProductId { get; init; }

        public GetApprovedCommentsQuery(int productId)
        {
            ProductId = productId;
        }
    }

    public class GetApprovedCommentsQueryHandler (AppDbContext _context) : IRequestHandler<GetApprovedCommentsQuery, List<CommentApprovedResponse>>
    {

        public async Task<List<CommentApprovedResponse>> Handle(GetApprovedCommentsQuery request, CancellationToken cancellationToken)
        {
            // فقط کامنت‌های تایید شده را برمی‌گرداند
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.ProductId == request.ProductId && c.IsApproved)
                .Include(c => c.User) //  برای نمایش نام کاربر
                .Select(c => new CommentApprovedResponse
                {
                    Id = c.Id,                //  نام‌گذاری صریح
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    ProductName = c.Product.Name, // اگر Product رو Include نکردی، این خط خطا میده!
                    Username = c.User.UserName
                })
                .ToListAsync(cancellationToken);
        }
    }
}