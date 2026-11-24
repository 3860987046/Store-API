using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.DTOs;
using StoreAPI.Models;

namespace StoreAPI.Command
{
    public class CreateOrderCommand : IRequest<int>
    {
        public string UserId { get; init; }
        public List<OrderItemDto> Items { get; init; }

        public CreateOrderCommand(string userId, List<OrderItemDto> items)
        {
            UserId = userId;
            Items = items;
        }
    }

    public class CreateOrderCommandHandler(AppDbContext _context) : IRequestHandler<CreateOrderCommand, int>
    {

        public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // ... (منطق قبلی بدون تغییر)
            var order = new Order
            {
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                if (product == null)
                    throw new Exception($"محصولی با شناسه {item.ProductId} یافت نشد.");

                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"موجودی محصول {product.Name} کافی نیست.");

                totalAmount += product.Price * item.Quantity;
                product.StockQuantity -= item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }

    // ==========================================================
    // 3️⃣ اعتبارسنج (Validator)
    // ==========================================================
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("شناسه کاربر الزامی است.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("لیست آیتم‌های سفارش نمی‌تواند خالی باشد.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("شناسه محصول معتبر نیست.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("تعداد محصول باید بیشتر از صفر باشد.");
            });
        }
    }
}