using MediatR;
using StoreAPI.Data;

namespace StoreAPI.Command.Comments
{
    public class DeleteCommentCommand : IRequest<bool>
    {
        public int CommentId { get; init; }

        public DeleteCommentCommand(int commentId)
        {
            CommentId = commentId;
        }
    }

    public class DeleteCommentCommandHandler(AppDbContext _context) : IRequestHandler<DeleteCommentCommand, bool>
    {

        public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.Comments.FindAsync(request.CommentId, cancellationToken);
            if (comment == null)
                throw new Exception("کامنت یافت نشد.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
