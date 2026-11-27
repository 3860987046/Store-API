using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Command.Orders
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public int OrderId { get; init; }
        public string UserId { get; init; }

        public DeleteOrderCommand(int orderId, string userId)
        {
            OrderId = orderId;
            UserId = userId;
        }
    }

    public class DeleteOrderCommandHandler(AppDbContext _context) : IRequestHandler<DeleteOrderCommand, bool>
    {

        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            // ... (منطق قبلی بدون تغییر)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

            if (order == null)
                throw new Exception("سفارش یافت نشد یا مال شما نیست.");

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                throw new Exception("این سفارش ارسال شده و قابل لغو نیست.");

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null) product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==========================================================
    // 3️⃣ اعتبارسنج (Validator)
    // ==========================================================
    public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
    {
        public DeleteOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("شناسه سفارش معتبر نیست.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("شناسه کاربر الزامی است.");
        }
    }
}