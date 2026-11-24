using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;

namespace StoreAPI.Command
{
    public class ApproveCommentCommand : IRequest<bool>
    {
        public int CommentId { get; init; }

        public ApproveCommentCommand(int commentId)
        {
            CommentId = commentId;
        }
    }

    public class ApproveCommentCommandHandler (AppDbContext _context) : IRequestHandler<ApproveCommentCommand, bool>
    {

        public async Task<bool> Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.Comments.FindAsync(request.CommentId, cancellationToken);
            if (comment == null)
                throw new Exception("کامنت یافت نشد.");

            comment.IsApproved = true; // 👈 تایید کامنت
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
