using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;

namespace StoreAPI.Command
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public int Id { get; init; } 

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }

    public class DeleteProductCommandHandler(AppDbContext _context) : IRequestHandler<DeleteProductCommand, bool>
    {
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(request.Id);
            if (product == null) throw new Exception("محصول یافت نشد.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}