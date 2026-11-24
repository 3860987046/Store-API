using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;

namespace StoreAPI.Query
{

    public class CommentUnApprovedResponse
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProductName { get; set; }
        public string Username { get; set; }
    }
    public class GetUnapprovedCommentsQuery : IRequest<List<CommentUnApprovedResponse>>
    {
    }

    public class GetUnapprovedCommentsQueryHandler (AppDbContext _context) : IRequestHandler<GetUnapprovedCommentsQuery, List<CommentUnApprovedResponse>>
    {

        public async Task<List<CommentUnApprovedResponse>> Handle(GetUnapprovedCommentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => !c.IsApproved)
                .Include(c => c.Product)
                .Include(c => c.User)
                .Select(c => new CommentUnApprovedResponse
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