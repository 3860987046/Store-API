using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.DTOs;
using StoreAPI.Models;

namespace StoreAPI.Command.Orders
{
    public class UpdateOrderCommand : IRequest<bool>
    {
        public int OrderId { get; init; }
        public string UserId { get; init; }
        public List<OrderItemDto> NewItems { get; init; }

        public UpdateOrderCommand(int orderId, string userId, List<OrderItemDto> newItems)
        {
            OrderId = orderId;
            UserId = userId;
            NewItems = newItems;
        }
    }

    public class UpdateOrderCommandHandler (AppDbContext _context) : IRequestHandler<UpdateOrderCommand, bool>
    {

        public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            // ... (منطق قبلی بدون تغییر)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

            if (order == null)
                throw new Exception("سفارش یافت نشد یا مال شما نیست.");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                throw new Exception("این سفارش قابل ویرایش نیست.");

            foreach (var oldItem in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(oldItem.ProductId);
                if (product != null) product.StockQuantity += oldItem.Quantity;
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems.Clear();

            decimal newTotal = 0;
            foreach (var newItem in request.NewItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == newItem.ProductId, cancellationToken);

                if (product == null)
                    throw new Exception($"محصولی با شناسه {newItem.ProductId} یافت نشد.");

                if (product.StockQuantity < newItem.Quantity)
                    throw new Exception($"موجودی محصول {product.Name} کافی نیست.");

                product.StockQuantity -= newItem.Quantity;
                newTotal += product.Price * newItem.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = newItem.Quantity,
                    UnitPrice = product.Price
                });
            }

            order.TotalAmount = newTotal;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==========================================================
    // 3️⃣ اعتبارسنج (Validator)
    // ==========================================================
    public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
    {
        public UpdateOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("شناسه سفارش معتبر نیست.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("شناسه کاربر الزامی است.");

            RuleFor(x => x.NewItems)
                .NotEmpty().WithMessage("لیست آیتم‌های جدید نمی‌تواند خالی باشد.");

            RuleForEach(x => x.NewItems).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("شناسه محصول معتبر نیست.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("تعداد محصول باید بیشتر از صفر باشد.");
            });
        }
    }
}