using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Query
{
    public class GetOrdersQuery : IRequest<IEnumerable<Order>> { };

    public class GetOrdersQueryHandler(AppDbContext _context) : IRequestHandler<GetOrdersQuery, IEnumerable<Order>>
    {

        public async Task<IEnumerable<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            //داده ها رو فقط میخوای بخونی پس تغییرات شون track نکن. 

            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(x => x.Product)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
