using FluentValidation;
using MediatR;
using StoreAPI.Data;
using StoreAPI.DTOs;
using StoreAPI.Models;

namespace StoreAPI.Command.Products
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public int StockQuantity { get; init; }

        public CreateProductCommand(string name, string description, decimal price, int stockQuantity)
        {
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
        }
    }

    public class CreateProductCommandHandler(AppDbContext _context) : IRequestHandler<CreateProductCommand, int>
    {

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            return product.Id;
        }
    }

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("نام محصول الزامی است.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("قیمت باید بیشتر از صفر باشد.");
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("موجودی نمی‌تواند منفی باشد.");
        }
    }
}