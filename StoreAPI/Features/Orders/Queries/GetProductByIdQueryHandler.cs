using MediatR;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Features.Orders.Queries
{

    //«من مسئول رسیدگی به GetProductByIdQuery هستم و خروجی من هم Product است»
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product?>

    {
        private readonly AppDbContext _context;
        public GetProductByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        //request: همون برگه سفارشه که اطلاعاتش مثل Id توشه
        //CancellationToken:یک مکانیزم امنیتی برای لغو عملیات در صورت نیاز
        public async Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Products.FindAsync(request.Id);
        }
    }
}
