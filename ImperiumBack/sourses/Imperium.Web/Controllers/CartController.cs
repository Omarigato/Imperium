using Microsoft.AspNetCore.Mvc;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Cart;
using Imperium.Service.Services.Cart;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с корзиной без авторизации (по clientId)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Получить корзину клиента
        /// </summary>
        [HttpGet("{clientId}")]
        public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCart(Guid clientId)
        {
            try
            {
                var cart = await _cartService.GetClientCartAsync(clientId);

                return Ok(new ApiResponse<CartSummaryDto>
                {
                    Success = true,
                    Data = cart,
                    Message = $"В корзине {cart.ItemsCount} товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<CartSummaryDto>
                {
                    Success = false,
                    Message = "Ошибка при получении корзины"
                });
            }
        }

        /// <summary>
        /// Добавить товар в корзину
        /// </summary>
        [HttpPost("{clientId}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart(Guid clientId, [FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var cartItem = await _cartService.AddToCartAsync(clientId, addToCartDto);

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cartItem,
                    Message = "Товар добавлен в корзину"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Ошибка при добавлении в корзину"
                });
            }
        }

        /// <summary>
        /// Обновить товар в корзине
        /// </summary>
        [HttpPut("{clientId}/item/{cartId}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(Guid clientId, Guid cartId, [FromBody] UpdateCartItemDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var cartItem = await _cartService.UpdateCartItemAsync(clientId, cartId, updateDto);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Товар не найден в корзине"
                    });
                }

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cartItem,
                    Message = "Товар обновлен в корзине"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartId} for client {ClientId}", cartId, clientId);
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Ошибка при обновлении товара в корзине"
                });
            }
        }

        /// <summary>
        /// Удалить товар из корзины
        /// </summary>
        [HttpDelete("{clientId}/item/{cartId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveFromCart(Guid clientId, Guid cartId)
        {
            try
            {
                var success = await _cartService.RemoveFromCartAsync(clientId, cartId);

                if (!success)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Товар не найден в корзине"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Товар удален из корзины"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart item {CartId} for client {ClientId}", cartId, clientId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении из корзины"
                });
            }
        }

        /// <summary>
        /// Очистить корзину
        /// </summary>
        [HttpDelete("{clientId}")]
        public async Task<ActionResult<ApiResponse<bool>>> ClearCart(Guid clientId)
        {
            try
            {
                var success = await _cartService.ClearCartAsync(clientId);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Корзина очищена" : "Ошибка при очистке корзины"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при очистке корзины"
                });
            }
        }

        /// <summary>
        /// Получить итоги корзины
        /// </summary>
        [HttpGet("{clientId}/summary")]
        public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCartSummary(Guid clientId)
        {
            try
            {
                var summary = await _cartService.GetCartSummaryAsync(clientId);

                return Ok(new ApiResponse<CartSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = $"Товаров: {summary.ItemsCount}, сумма: {summary.TotalAmount:N0} ₸"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<CartSummaryDto>
                {
                    Success = false,
                    Message = "Ошибка при получении итогов корзины"
                });
            }
        }

        /// <summary>
        /// Проверить, есть ли товар в корзине
        /// </summary>
        [HttpGet("{clientId}/check/{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckIfInCart(Guid clientId, Guid productId)
        {
            try
            {
                var isInCart = await _cartService.IsProductInCartAsync(clientId, productId);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isInCart,
                    Message = isInCart ? "Товар в корзине" : "Товар не в корзине"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart for client {ClientId}, product {ProductId}", clientId, productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при проверке корзины"
                });
            }
        }

        /// <summary>
        /// Получить количество товаров в корзине
        /// </summary>
        [HttpGet("{clientId}/count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartItemsCount(Guid clientId)
        {
            try
            {
                var count = await _cartService.GetCartItemsCountAsync(clientId);

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = $"В корзине {count} товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Ошибка при подсчете товаров в корзине"
                });
            }
        }
    }
}