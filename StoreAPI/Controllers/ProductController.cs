using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.DTOs;
using StoreAPI.Command.Products; 
using StoreAPI.Query.Products;  

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ==========================================
        // 1. گرفتن لیست همه محصولات (Get All)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetProducts(CancellationToken token)
        {
            var products = await _mediator.Send(new GetAllProductsQuery(), token);
            return Ok(products);
        }

        // ==========================================
        // 2. گرفتن محصول با آیدی (Get By Id)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id, CancellationToken token)
        {
            try
            {
                var product = await _mediator.Send(new GetProductByIdQuery(id), token);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 3. ساخت محصول جدید (Create) - فقط ادمین
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")] //  فقط ادمین بتونه محصول بسازه
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto, CancellationToken token)
        {
            try
            {
                var productId = await _mediator.Send(new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.StockQuantity), token);

                return Ok(new { Message = "محصول با موفقیت ساخته شد", ProductId = productId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 4. ویرایش محصول (Update) - فقط ادمین
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto, CancellationToken token)
        {
            try
            {
                await _mediator.Send(new UpdateProductCommand(
                            id,
                            dto.Name,
                            dto.Description,
                            dto.Price,
                            dto.StockQuantity
                        ), token); return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==========================================
        // 5. حذف محصول (Delete) - فقط ادمین
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken token)
        {
            try
            {
                await _mediator.Send(new DeleteProductCommand(id), token);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}