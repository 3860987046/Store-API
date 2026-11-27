using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.Command.Comments;
using StoreAPI.DTOs;
using StoreAPI.Query.Comments;
using System.Security.Claims;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ==========================================
        // 1. ثبت کامنت (کاربر لاگین کرده)
        // ==========================================
        [HttpPost]
        [Authorize] //  کاربر باید لاگین باشد
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto, CancellationToken token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var commentId = await _mediator.Send(new AddCommentCommand(userId, dto.ProductId, dto.Text), token);
                return Ok(new { Message = "کامنت ثبت شد و در انتظار تایید است.", CommentId = commentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 2. گرفتن کامنت‌های تایید شده (برای همه)
        // ==========================================
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetComments(int productId, CancellationToken token)
        {
            var comments = await _mediator.Send(new GetApprovedCommentsQuery(productId), token);
            return Ok(comments);
        }

        // ==========================================
        // 3. گرفتن کامنت‌های تایید نشده (فقط ادمین)
        // ==========================================
        [HttpGet("unapproved")]
        [Authorize(Roles = "Admin")] // 👈 فقط ادمین
        public async Task<IActionResult> GetUnapprovedComments(CancellationToken token)
        {
            var comments = await _mediator.Send(new GetUnapprovedCommentsQuery(), token);
            return Ok(comments);
        }

        // ==========================================
        // 4. تایید کامنت (فقط ادمین)
        // ==========================================
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveComment(int id, CancellationToken token)
        {
            try
            {
                await _mediator.Send(new ApproveCommentCommand(id), token);
                return Ok(new { Message = "کامنت تایید شد." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 5. حذف کامنت (فقط ادمین)
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComment(int id, CancellationToken token)
        {
            try
            {
                await _mediator.Send(new DeleteCommentCommand(id), token);
                return Ok(new { Message = "کامنت حذف شد." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}