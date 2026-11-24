using FluentValidation;
using MediatR;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Command
{
    public class AddCommentCommand : IRequest<int>
    {
        public string UserId { get; init; }
        public int ProductId { get; init; }
        public string Text { get; init; }

        public AddCommentCommand(string userId, int productId, string text)
        {
            UserId = userId;
            ProductId = productId;
            Text = text;
        }
    }

    public class AddCommentCommandHandler (AppDbContext _context) : IRequestHandler<AddCommentCommand, int>
    {

        public async Task<int> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            // 1. چک کردن وجود محصول
            var product = await _context.Products.FindAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new Exception("محصولی با این شناسه یافت نشد.");

            // 2. ساخت کامنت جدید (تایید نشده)
            var comment = new Comment
            {
                Text = request.Text,
                UserId = request.UserId,
                ProductId = request.ProductId,
                IsApproved = false //  پیش‌فرض تایید نشده
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(cancellationToken);

            return comment.Id;
        }
    }

    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("شناسه کاربر الزامی است.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("شناسه محصول معتبر نیست.");
            RuleFor(x => x.Text).NotEmpty().WithMessage("متن کامنت الزامی است.")
                               .MaximumLength(500).WithMessage("متن کامنت نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");
        }
    }
}