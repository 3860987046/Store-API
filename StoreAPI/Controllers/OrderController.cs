using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using System.Security.Claims;
using StoreAPI.DTOs;
using StoreAPI.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OrderManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
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
        [HttpPost] // این متد با درخواست POST اجرا می‌شود
        public async Task<IActionResult> CreateOrder(
                   [FromBody] CreateOrderDto dto) // دریافت اطلاعات سفارش از بدنه درخواست
        {
            // مرحله 1: دریافت شناسه کاربر احراز هویت‌شده
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier); // خواندن شناسه از Claim استاندارد

            // بررسی می‌کنیم شناسه وجود داشته و یک عدد معتبر باشد
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(); // در صورت نبود شناسه معتبر، پاسخ 401
            }

            // مرحله 2: ساخت یک سفارش جدید در حافظه
            var order = new Order
            {
                UserId = userId, // اتصال سفارش به کاربر فعلی
                OrderDate = DateTime.UtcNow, // ثبت زمان فعلی بر اساس UTC
                TotalAmount = 0 // مبلغ ابتدا صفر است و بعد محاسبه می‌شود
            };

            // مرحله 3: بررسی تک‌تک محصولات ارسال‌شده
            foreach (var item in dto.Items)
            {
                // محصول را با شناسه ارسالی از دیتابیس پیدا می‌کنیم
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == item.ProductId);

                // اگر محصول در دیتابیس وجود نداشته باشد
                if (product == null)
                {
                    return BadRequest(
                        $"Product {item.ProductId} not found."); // پاسخ 400
                }

                // بررسی می‌کنیم موجودی برای تعداد درخواستی کافی باشد
                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest(
                        $"Insufficient stock for {product.Name}."); // موجودی ناکافی
                }

                // مرحله 4: خواندن قیمت معتبر از دیتابیس
                var unitPrice = product.Price; // قیمت را از ورودی کاربر نمی‌گیریم

                // محاسبه مبلغ این ردیف: قیمت واحد ضربدر تعداد
                var lineTotal = unitPrice * item.Quantity;

                // ایجاد ردیف سفارش برای محصول فعلی
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id, // شناسه محصول معتبر از دیتابیس
                    Quantity = item.Quantity, // تعداد اعتبارسنجی‌شده
                    UnitPrice = unitPrice // ذخیره قیمت واقعی در زمان سفارش
                });

                // مرحله 5: افزودن مبلغ این ردیف به مبلغ کل سفارش
                order.TotalAmount += lineTotal;

                // کاهش موجودی محصول در حافظه
                product.StockQuantity -= item.Quantity;
            }

            // مرحله 6: اضافه‌کردن سفارش به مجموعه سفارش‌های EF Core
            _context.Orders.Add(order);

            // ذخیره سفارش، ردیف‌های سفارش و تغییرات موجودی در دیتابیس
            await _context.SaveChangesAsync();

            // ارسال پاسخ موفق همراه با اطلاعات اصلی سفارش
            return Ok(new
            {
                order.Id, // شناسه‌ای که دیتابیس برای سفارش ساخته است
                order.UserId, // شناسه کاربر سفارش‌دهنده
                order.TotalAmount, // مبلغ محاسبه‌شده در سمت سرور
                order.OrderDate // زمان ثبت سفارش
            });
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
                        order.UserId = Convert.ToInt32(update.Value.ToString());
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
