using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Imperium.Service.Services;
using Imperium.Service.DTOs.Product;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeatured()
        {
            var products = await _productService.GetFeaturedAsync();
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(Guid categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<ProductDto>> GetByCode(string code)
        {
            var product = await _productService.GetByCodeAsync(code);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search(string searchTerm)
        {
            var products = await _productService.SearchAsync(searchTerm);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.CreateAsync(createProductDto);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] CreateProductDto updateProductDto)
        {
            try
            {
                var product = await _productService.UpdateAsync(id, updateProductDto);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}