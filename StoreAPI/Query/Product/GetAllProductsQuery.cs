using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Query.Product
{
    public class GetAllProductsQuery : IRequest<List<Product>> { }

    public class GetAllProductsQueryHandler(AppDbContext _context) : IRequestHandler<GetAllProductsQuery, List<Product>>
    {

        public async Task<List<Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Products.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}