using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.Command;
using StoreAPI.DTOs;
using System.Security.Claims;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 👈 همه متدهای این کنترلر نیاز به لاگین دارند
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ==========================================
        // 1. ثبت سفارش (Create)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken token)
        {
            // گرفتن آیدی کاربر از توکن
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var orderId = await _mediator.Send(new CreateOrderCommand(userId, dto.Items), token);
                return Ok(new { Message = "سفارش با موفقیت ثبت شد", OrderId = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 2. ویرایش کامل سفارش (Update)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto, CancellationToken token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _mediator.Send(new UpdateOrderCommand(id, userId, dto.NewItems), token);
                return NoContent(); // 204 - موفقیت بدون محتوا
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 3. ویرایش جزئی (Patch) - فقط تعداد یک آیتم
        // ==========================================
        [HttpPatch("{id}/items/{itemId}")]
        public async Task<IActionResult> PatchOrderItem(int id, int itemId, [FromBody] PatchOrderItemDto dto, CancellationToken token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _mediator.Send(new PatchOrderCommand(id, itemId, dto.NewQuantity, userId), token);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 4. حذف/لغو سفارش (Delete)
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _mediator.Send(new DeleteOrderCommand(id, userId), token);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}