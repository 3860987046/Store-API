using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Query
{
    public class GetProductByIdQuery : IRequest<Product>
    {
        public int Id { get; init; }
        public GetProductByIdQuery(int id) { Id = id; }
    }

    public class GetProductByIdQueryHandler (AppDbContext _context) : IRequestHandler<GetProductByIdQuery, Product>
    {

        public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                throw new Exception("محصولی با این شناسه یافت نشد.");

            return product;
        }
    }
}