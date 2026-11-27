using FluentValidation;
using MediatR;
using StoreAPI.Data;
using StoreAPI.DTOs;

namespace StoreAPI.Command.Products
{
    public class UpdateProductCommand : IRequest<bool>
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public int StockQuantity { get; init; }

        public UpdateProductCommand(int id, string name, string description, decimal price, int stockQuantity)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
        }
    }

    public class UpdateProductCommandHandler(AppDbContext _context) : IRequestHandler<UpdateProductCommand, bool>
    {

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(request.Id);
            if (product == null) throw new Exception("محصول یافت نشد.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("شناسه محصول معتبر نیست.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("نام محصول الزامی است.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("قیمت باید بیشتر از صفر باشد.");
        }
    }
}