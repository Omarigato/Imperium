using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Favorite;
using Imperium.Service.Services.Favorite;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с избранным без авторизации (по clientId)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IFavoriteService favoriteService, ILogger<FavoriteController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        /// <summary>
        /// Получить избранные товары клиента
        /// </summary>
        [HttpGet("{clientId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FavoriteDto>>>> GetFavorites(Guid clientId)
        {
            try
            {
                var favorites = await _favoriteService.GetClientFavoritesAsync(clientId);

                return Ok(new ApiResponse<IEnumerable<FavoriteDto>>
                {
                    Success = true,
                    Data = favorites,
                    Message = $"Найдено {favorites.Count()} товаров в избранном"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<IEnumerable<FavoriteDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении избранного"
                });
            }
        }

        /// <summary>
        /// Добавить товар в избранное
        /// </summary>
        [HttpPost("{clientId}")]
        public async Task<ActionResult<ApiResponse<FavoriteDto>>> AddToFavorite(Guid clientId, [FromBody] AddToFavoriteDto addDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<FavoriteDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var favorite = await _favoriteService.AddToFavoriteAsync(clientId, addDto);

                return Ok(new ApiResponse<FavoriteDto>
                {
                    Success = true,
                    Data = favorite,
                    Message = "Товар добавлен в избранное"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<FavoriteDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to favorites for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<FavoriteDto>
                {
                    Success = false,
                    Message = "Ошибка при добавлении в избранное"
                });
            }
        }

        /// <summary>
        /// Удалить товар из избранного
        /// </summary>
        [HttpDelete("{clientId}/product/{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveFromFavorite(Guid clientId, Guid productId)
        {
            try
            {
                var success = await _favoriteService.RemoveFromFavoriteAsync(clientId, productId);

                if (!success)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Товар не найден в избранном"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Товар удален из избранного"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from favorites for client {ClientId}, product {ProductId}", clientId, productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении из избранного"
                });
            }
        }

        /// <summary>
        /// Проверить, есть ли товар в избранном
        /// </summary>
        [HttpGet("{clientId}/check/{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckIfInFavorites(Guid clientId, Guid productId)
        {
            try
            {
                var isInFavorites = await _favoriteService.IsProductInFavoritesAsync(clientId, productId);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isInFavorites,
                    Message = isInFavorites ? "Товар в избранном" : "Товар не в избранном"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking favorites for client {ClientId}, product {ProductId}", clientId, productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при проверке избранного"
                });
            }
        }

        /// <summary>
        /// Получить количество товаров в избранном
        /// </summary>
        [HttpGet("{clientId}/count")]
        public async Task<ActionResult<ApiResponse<int>>> GetFavoriteCount(Guid clientId)
        {
            try
            {
                var count = await _favoriteService.GetFavoriteCountAsync(clientId);

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = $"В избранном {count} товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite count for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Ошибка при подсчете избранного"
                });
            }
        }

        /// <summary>
        /// Очистить избранное
        /// </summary>
        [HttpDelete("{clientId}")]
        public async Task<ActionResult<ApiResponse<bool>>> ClearFavorites(Guid clientId)
        {
            try
            {
                var success = await _favoriteService.ClearFavoritesAsync(clientId);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Избранное очищено" : "Ошибка при очистке избранного"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing favorites for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при очистке избранного"
                });
            }
        }
    }
}
