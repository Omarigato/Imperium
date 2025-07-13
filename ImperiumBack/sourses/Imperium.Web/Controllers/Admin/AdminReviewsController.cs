using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Review;
using Imperium.Service.Services.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для управления отзывами
    /// </summary>
    [ApiController]
    [Route("api/admin/reviews")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<AdminReviewsController> _logger;

        public AdminReviewsController(IReviewService reviewService, ILogger<AdminReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все отзывы
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();

                return Ok(new ApiResponse<IEnumerable<ReviewDto>>
                {
                    Success = true,
                    Data = reviews,
                    Message = $"Найдено {reviews.Count()} отзывов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reviews");
                return StatusCode(500, new ApiResponse<IEnumerable<ReviewDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении отзывов"
                });
            }
        }

        /// <summary>
        /// Удалить отзыв
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReview(Guid id)
        {
            try
            {
                var success = await _reviewService.AdminDeleteReviewAsync(id);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Отзыв удален" : "Отзыв не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении отзыва"
                });
            }
        }

        /// <summary>
        /// Получить статистику отзывов
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<ReviewStatisticsDto>>> GetReviewStatistics()
        {
            try
            {
                var statistics = await _reviewService.GetReviewStatisticsAsync();

                return Ok(new ApiResponse<ReviewStatisticsDto>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Статистика отзывов получена"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics");
                return StatusCode(500, new ApiResponse<ReviewStatisticsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики"
                });
            }
        }
    }
}
