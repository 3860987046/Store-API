using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Query.Orders
{
    public class GetOrderByIdQuery : IRequest<Order?>
    {
        public int Id { get; }
        public GetOrderByIdQuery(int id)
        {
            Id = id;
        }
    }

    public class GetOrderByIdQueryHandler(AppDbContext _context) : IRequestHandler<GetOrderByIdQuery, Order?>
    {
        public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken token)
        {
            return await _context.Orders
                 .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                 .Include(o => o.User)
                 .AsNoTracking()
                 .FirstOrDefaultAsync(o => o.Id == request.Id, token);
        }
    }

}
