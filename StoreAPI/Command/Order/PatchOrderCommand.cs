using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Command
{
    public class PatchOrderCommand : IRequest<bool>
    {
        public int OrderId { get; init; }
        public int OrderItemId { get; init; }
        public int NewQuantity { get; init; }
        public string UserId { get; init; }

        public PatchOrderCommand(int orderId, int orderItemId, int newQuantity, string userId)
        {
            OrderId = orderId;
            OrderItemId = orderItemId;
            NewQuantity = newQuantity;
            UserId = userId;
        }
    }

    public class PatchOrderCommandHandler (AppDbContext _context) : IRequestHandler<PatchOrderCommand, bool>
    {

        public async Task<bool> Handle(PatchOrderCommand request, CancellationToken cancellationToken)
        {
            // ... (منطق قبلی بدون تغییر)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

            if (order == null)
                throw new Exception("سفارش یافت نشد یا مال شما نیست.");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                throw new Exception("این سفارش قابل ویرایش نیست.");

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == request.OrderItemId);
            if (orderItem == null)
                throw new Exception("آیتم مورد نظر در این سفارش یافت نشد.");

            var diff = request.NewQuantity - orderItem.Quantity;
            if (diff == 0) return true;

            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product == null)
                throw new Exception("محصول یافت نشد.");

            if (diff > 0)
            {
                if (product.StockQuantity < diff)
                    throw new Exception($"موجودی محصول {product.Name} کافی نیست.");
                product.StockQuantity -= diff;
            }
            else
            {
                product.StockQuantity += Math.Abs(diff);
            }

            orderItem.Quantity = request.NewQuantity;
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==========================================================
    // 3️⃣ اعتبارسنج (Validator)
    // ==========================================================
    public class PatchOrderCommandValidator : AbstractValidator<PatchOrderCommand>
    {
        public PatchOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("شناسه سفارش معتبر نیست.");

            RuleFor(x => x.OrderItemId)
                .GreaterThan(0).WithMessage("شناسه آیتم سفارش معتبر نیست.");

            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("تعداد جدید باید بیشتر از صفر باشد.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("شناسه کاربر الزامی است.");
        }
    }
}