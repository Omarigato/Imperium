using Imperium.Service.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Review
{
    public interface IReviewService
    {
        // Публичные методы
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId);
        Task<IEnumerable<ReviewDto>> GetClientReviewsAsync(Guid clientId);
        Task<double> GetProductAverageRatingAsync(Guid productId);
        Task<ProductReviewStatsDto> GetProductReviewStatsAsync(Guid productId);

        // Создание и управление отзывами
        Task<ReviewDto> CreateReviewAsync(Guid clientId, CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(Guid reviewId, Guid clientId, UpdateReviewDto updateReviewDto);
        Task<bool> DeleteReviewAsync(Guid reviewId, Guid clientId);
        Task<bool> CanClientReviewProductAsync(Guid clientId, Guid productId);

        // Админские методы
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetReviewsByRatingAsync(int rating);
        Task<bool> AdminDeleteReviewAsync(Guid reviewId);
        Task<ReviewStatisticsDto> GetReviewStatisticsAsync();
    }
}
