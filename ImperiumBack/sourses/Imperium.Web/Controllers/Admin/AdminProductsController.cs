using Imperium.Core.Models;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Product;
using Imperium.Service.Services.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для управления товарами
    /// </summary>
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<AdminProductsController> _logger;

        public AdminProductsController(IProductService productService, ILogger<AdminProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все товары
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllAsync();

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Data = products,
                    Message = $"Найдено {products.Count()} товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении товаров"
                });
            }
        }

        /// <summary>
        /// Создать товар
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                var authorId = GetCurrentUserId();
                var product = await _productService.CreateAsync(createDto);

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Товар создан успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Ошибка при создании товара"
                });
            }
        }

        /// <summary>
        /// Получить товар по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Товар не найден"
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Ошибка при получении товара"
                });
            }
        }

        /// <summary>
        /// Удалить товар
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(Guid id)
        {
            try
            {
                var success = await _productService.DeleteAsync(id);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Товар удален" : "Товар не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении товара"
                });
            }
        }

        /// <summary>
        /// Переключить доступность товара
        /// </summary>
        [HttpPost("{id}/toggle-availability")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleAvailability(Guid id)
        {
            try
            {
                var success = await _productService.ToggleAvailabilityAsync(id);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Доступность товара изменена" : "Товар не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling availability for product {ProductId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при изменении доступности товара"
                });
            }
        }

        /// <summary>
        /// Получить статистику товаров
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<ProductStatisticsDto>>> GetProductStatistics()
        {
            try
            {
                var statistics = await _productService.GetProductStatisticsAsync();

                return Ok(new ApiResponse<ProductStatisticsDto>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Статистика товаров получена"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product statistics");
                return StatusCode(500, new ApiResponse<ProductStatisticsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}
