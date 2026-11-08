using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Features.Orders.Commands
{
    //آشپز ثبت سفارش که میگه طبق قانون RequestHandler برگه سفارش رو میگیرم و بعد شماره سفارش ثبت شده رو برمیگردونم 
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
    {
        private readonly AppDbContext _context; //  ابزار دسترسی به دیتابیس

        public CreateOrderCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateOrderCommand request , CancellationToken cancellationToken)
        {

            // 1. یک شیء سفارش جدید می‌سازیم. مبلغ فعلاً صفر
            var order = new Order
            {
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            decimal totalAmount = 0; 
           // 2. روی تک‌تک محصولاتی که کاربر انتخاب کرده حلقه می‌زنیم
            foreach (var item in request.Items)
            {
                // پیدا کردن محصول توی دیتابیس
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                // اگر محصول پیدا نشد، خطا میدیم
                if (product == null)
                    throw new Exception($"محصولی با شناسه {item.ProductId} یافت نشد.");

                // چک کردن موجودی انبار
                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"موجودی محصول {product.Name} کافی نیست.");

                // محاسبه قیمت: قیمت محصول × تعداد
                totalAmount += product.Price * item.Quantity;

                // اضافه کردن این محصول به لیست آیتم‌های سفارش
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price // قیمت رو در لحظه خرید ذخیره میکنیم (نه بعداً)
                });

                // کم کردن موجودی محصول از انبار
                product.StockQuantity -= item.Quantity;
            }

            // 3. مبلغ کل سفارش رو ست میکنیم
            order.TotalAmount = totalAmount;

            // 4. سفارش رو به دیتابیس اضافه میکنیم و ذخیره میکنیم
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. آیدی سفارش رو برمی‌گردونیم
            return order.Id;
        }
    }
}
