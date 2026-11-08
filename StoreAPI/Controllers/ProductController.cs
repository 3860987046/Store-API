using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.DTOs;
using StoreAPI.Models;
using StoreAPI.Features.Orders.Queries;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;
        public ProductController(AppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }
        /*
        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return product;
        }*/

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
            return Ok(product);
        }

        // GET: api/products/filter?minPrice=10&maxPrice=100
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Product>>> FilterByPrice(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.ToListAsync();
        }

        // Post: api/products/Post
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (dto == null)
                return BadRequest("داده‌های ورودی نامعتبره");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
               
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            //این خط به کلاینت (اپلیکیشن) می‌گه: «محصول با موفقیت ساخته شد! این هم اطلاعاتش. اگر می‌خوای همین الان اطلاعاتش رو ببینی، برو به این آدرس (URL) که بهت می‌دم.»

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (dto == null)
                return BadRequest("داده‌های ورودی نامعتبر است");

            // ۱. محصول رو از دیتابیس پیدا کن
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound($"محصولی با شناسه {id} یافت نشد");

            // ۲. فیلدها رو به‌روزرسانی کن
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;


            // ۳. ذخیره کن
            await _context.SaveChangesAsync();

            return Ok(product);   // یا NoContent() اگه نمی‌خوای بدنه برگرده
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound($"محصولی با شناسه {id} یافت نشد");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
