using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using System.Security.Claims;
using MediatR;
using StoreAPI.DTOs;
using StoreAPI.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using StoreAPI.Features.Orders.Commands;

namespace OrderManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;

        public OrdersController(AppDbContext context , IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(x=>x.Product)
                .ToListAsync();
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                    .Include(o => o.OrderItems)          // اول آیتم‌های سفارش
                    .ThenInclude(oi => oi.Product)      // بعد محصول هر آیتم
                    .Include(o => o.User)               // و کاربر سفارش
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }
            return order;
        }

        // POST: api/orders
        [HttpPost] 
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto) 
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userIdValue))
            {
                return Unauthorized(); 
            }
            //  گارسون برگه سفارش رو میفرسته به صندوق
            try
            {
                var orderId = await _mediator.Send(new CreateOrderCommand(userIdValue, dto.Items));
                return Ok(new { Message = "سفارش با موفقیت ثبت شد", OrderId = orderId });
            }
            catch (Exception ex)
            {
                // اگر آشپز خطا داد (مثلاً موجودی کم بود)، پیام رو به کاربر میدیم
                return BadRequest(new { Message = ex.Message });
            }

        }
   


        // PUT: api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ─────────────────────────────────────────────────────────────
        // PATCH: api/orders/5 (برای update جزئی)
        // این endpoint فقط بخشی از فیلدهای یک سفارش را به‌روزرسانی می‌کند،
        // نه کل آبجکت را (برخلاف PUT که کل موجودیت را جایگزین می‌کند)
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("{id}")]   
        public async Task<IActionResult> PatchOrder(int id,
            [FromBody] Dictionary<string, object> updates)   // بدنه‌ی درخواست: دیکشنری «نام فیلد ← مقدار جدید»
                                                             // چون نمی‌دانیم کدام فیلدها فرستاده می‌شوند، از Dictionary استفاده شده
        {
            // جستجوی سفارش در دیتابیس بر اساس شناسه (id)
            // FindAsync اول در حافظه (tracked entities) می‌گردد، بعد دیتابیس
            var order = await _context.Orders.FindAsync(id);

            // اگر سفارشی با این id پیدا نشد، پاسخ 404 (NotFound) برگردان
            if (order == null)
                return NotFound();

            // پیمایش روی تک‌تک فیلدهای ارسال‌شده در بدنه‌ی درخواست
            foreach (var update in updates)
            {
                // بررسی نام فیلد (کلید دیکشنری) — به حروف کوچک تبدیل می‌شود
                // تا مقایسه case-insensitive باشد (UserId و userid یکی باشند)
                switch (update.Key.ToLower())
                {
                    // اگر فیلد ارسال‌شده «userid» بود:
                    case "userid":
                        // مقدار را به رشته تبدیل و سپس به int تبدیل و در UserId سفارش ذخیره کن
                        // ToString() چون مقدار از JSON به‌صورت object آمده ممکن است نوعش مشخص نباشد
                        order.UserId = update.Value.ToString();
                        break;

                    // اگر فیلد ارسال‌شده «totalamount» بود:
                    case "totalamount":
                        // مقدار را به decimal تبدیل و در TotalAmount سفارش ذخیره کن
                        // decimal برای مقادیر پولی مناسب‌تر از double/float است (دقت بالاتر)
                        order.TotalAmount = Convert.ToDecimal(update.Value.ToString());
                        break;

                    // اگر فیلد ارسال‌شده «orderdate» بود:
                    case "orderdate":
                        // مقدار را به DateTime تبدیل و در OrderDate سفارش ذخیره کن
                        order.OrderDate = Convert.ToDateTime(update.Value.ToString());
                        break;

                    // هر فیلد دیگری غیرمجاز است:
                    default:
                        // پاسخ 400 (BadRequest) با پیام خطای فارسی برگردان
                        return BadRequest($"فیلد '{update.Key}' قابل ویرایش نیست");
                }
            }

            // ذخیره‌ی تغییرات در دیتابیس (اجرای UPDATE)
            // تا این لحظه تغییرات فقط روی شیء در حافظه بود
            await _context.SaveChangesAsync();

            // پاسخ 204 (NoContent): عملیات موفق بود اما بدنه‌ای برای برگرداندن نیست
            return NoContent();
        }

        // ─────────────────────────────────────────────────────────────
        // متد کمکی OrderExists
        // هدف: بررسی وجود یک سفارش با شناسه‌ی مشخص در دیتابیس
        // خروجی: true اگر وجود داشته باشد، در غیر این صورت false
        // نکته: این متد فقط کوئری می‌زند و شیء را برنمی‌گرداند (سبک‌تر از FindAsync)
        // ─────────────────────────────────────────────────────────────
        private bool OrderExists(int id)
        {
            // Any روی مجموعه‌ی Orders بررسی می‌کند آیا سفارشی با Id برابر id وجود دارد
            // در صورت وجود، true برمی‌گرداند (بدون بارگذاری کامل شیء)
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
