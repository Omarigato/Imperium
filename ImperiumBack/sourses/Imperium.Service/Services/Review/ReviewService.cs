using AutoMapper;
using Imperium.Data.Repositories.Client;
using Imperium.Data.Repositories.Product;
using Imperium.Data.Repositories.Review;
using Imperium.Service.DTOs.Review;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Review
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepository,
            IClientRepository clientRepository,
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _clientRepository = clientRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId)
        {
            try
            {
                var reviews = await _reviewRepository.GetByProductIdWithDetailsAsync(productId);
                var result = new List<ReviewDto>();

                foreach (var review in reviews)
                {
                    var client = await _clientRepository.GetByIdAsync(review.ClientId);

                    result.Add(new ReviewDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        ClientId = review.ClientId,
                        ClientName = client?.FullName ?? "Аноним",
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreateDate = review.CreateDate
                    });
                }

                return result.OrderByDescending(r => r.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product {ProductId}", productId);
                return new List<ReviewDto>();
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetClientReviewsAsync(Guid clientId)
        {
            try
            {
                var reviews = await _reviewRepository.GetByClientIdAsync(clientId);
                var result = new List<ReviewDto>();

                foreach (var review in reviews)
                {
                    var product = await _productRepository.GetByIdAsync(review.ProductId);

                    result.Add(new ReviewDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        ProductName = product?.NameRu ?? "Неизвестный товар",
                        ProductCode = product?.Code ?? "",
                        ClientId = review.ClientId,
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreateDate = review.CreateDate
                    });
                }

                return result.OrderByDescending(r => r.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for client {ClientId}", clientId);
                return new List<ReviewDto>();
            }
        }

        public async Task<double> GetProductAverageRatingAsync(Guid productId)
        {
            try
            {
                return await _reviewRepository.GetAverageRatingByProductIdAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average rating for product {ProductId}", productId);
                return 0;
            }
        }

        public async Task<ProductReviewStatsDto> GetProductReviewStatsAsync(Guid productId)
        {
            try
            {
                var reviews = await _reviewRepository.GetByProductIdAsync(productId);
                var reviewsList = reviews.ToList();

                if (!reviewsList.Any())
                {
                    return new ProductReviewStatsDto
                    {
                        ProductId = productId,
                        TotalReviews = 0,
                        AverageRating = 0,
                        RatingDistribution = new Dictionary<int, int>
                        {
                            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                        }
                    };
                }

                var ratingDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    ratingDistribution[i] = reviewsList.Count(r => r.Rating == i);
                }

                return new ProductReviewStatsDto
                {
                    ProductId = productId,
                    TotalReviews = reviewsList.Count,
                    AverageRating = reviewsList.Average(r => r.Rating),
                    RatingDistribution = ratingDistribution,
                    LatestReviewDate = reviewsList.Max(r => r.CreateDate)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review stats for product {ProductId}", productId);
                return new ProductReviewStatsDto { ProductId = productId };
            }
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid clientId, CreateReviewDto createReviewDto)
        {
            try
            {
                // Проверяем, существует ли клиент
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client == null)
                {
                    throw new InvalidOperationException("Клиент не найден");
                }

                // Проверяем, существует ли товар
                var product = await _productRepository.GetByIdAsync(createReviewDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException("Товар не найден");
                }

                // Проверяем, не оставлял ли клиент уже отзыв на этот товар
                var existingReview = await _reviewRepository.GetByClientAndProductAsync(clientId, createReviewDto.ProductId);
                if (existingReview != null)
                {
                    throw new InvalidOperationException("Вы уже оставили отзыв на этот товар");
                }

                var review = new Core.Models.Review
                {
                    ClientId = clientId,
                    ProductId = createReviewDto.ProductId,
                    Rating = createReviewDto.Rating,
                    Comment = createReviewDto.Comment,
                    CreateDate = DateTime.UtcNow
                };

                await _reviewRepository.Insert(review);

                _logger.LogInformation("Review created: {ReviewId} by client {ClientId} for product {ProductId}",
                    review.Id, clientId, createReviewDto.ProductId);

                return new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    ProductName = product.NameRu,
                    ProductCode = product.Code,
                    ClientId = review.ClientId,
                    ClientName = client.FullName,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreateDate = review.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for product {ProductId} by client {ClientId}",
                    createReviewDto.ProductId, clientId);
                throw;
            }
        }

        public async Task<ReviewDto?> UpdateReviewAsync(Guid reviewId, Guid clientId, UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null || review.ClientId != clientId)
                {
                    return null;
                }

                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;

                await _reviewRepository.Update(review);

                _logger.LogInformation("Review updated: {ReviewId} by client {ClientId}", reviewId, clientId);

                // Получаем дополнительную информацию для DTO
                var client = await _clientRepository.GetByIdAsync(clientId);
                var product = await _productRepository.GetByIdAsync(review.ProductId);

                return new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    ProductName = product?.NameRu ?? "",
                    ProductCode = product?.Code ?? "",
                    ClientId = review.ClientId,
                    ClientName = client?.FullName ?? "",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreateDate = review.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId} by client {ClientId}", reviewId, clientId);
                return null;
            }
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid clientId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null || review.ClientId != clientId)
                {
                    return false;
                }

                await _reviewRepository.DeleteAsync(reviewId);

                _logger.LogInformation("Review deleted: {ReviewId} by client {ClientId}", reviewId, clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} by client {ClientId}", reviewId, clientId);
                return false;
            }
        }

        public async Task<bool> CanClientReviewProductAsync(Guid clientId, Guid productId)
        {
            try
            {
                // Проверяем, нет ли уже отзыва от этого клиента на этот товар
                var existingReview = await _reviewRepository.GetByClientAndProductAsync(clientId, productId);

                // TODO: Добавить проверку, покупал ли клиент этот товар
                // Пока разрешаем всем оставлять отзывы

                return existingReview == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if client {ClientId} can review product {ProductId}", clientId, productId);
                return false;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            try
            {
                var reviews = await _reviewRepository.GetAllAsync();
                var result = new List<ReviewDto>();

                foreach (var review in reviews)
                {
                    var client = await _clientRepository.GetByIdAsync(review.ClientId);
                    var product = await _productRepository.GetByIdAsync(review.ProductId);

                    result.Add(new ReviewDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        ProductName = product?.NameRu ?? "Неизвестный товар",
                        ProductCode = product?.Code ?? "",
                        ClientId = review.ClientId,
                        ClientName = client?.FullName ?? "Аноним",
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreateDate = review.CreateDate
                    });
                }

                return result.OrderByDescending(r => r.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reviews");
                return new List<ReviewDto>();
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByRatingAsync(int rating)
        {
            try
            {
                var reviews = await _reviewRepository.GetByRatingAsync(rating);
                var result = new List<ReviewDto>();

                foreach (var review in reviews)
                {
                    var client = await _clientRepository.GetByIdAsync(review.ClientId);
                    var product = await _productRepository.GetByIdAsync(review.ProductId);

                    result.Add(new ReviewDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        ProductName = product?.NameRu ?? "Неизвестный товар",
                        ProductCode = product?.Code ?? "",
                        ClientId = review.ClientId,
                        ClientName = client?.FullName ?? "Аноним",
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreateDate = review.CreateDate
                    });
                }

                return result.OrderByDescending(r => r.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews by rating {Rating}", rating);
                return new List<ReviewDto>();
            }
        }

        public async Task<bool> AdminDeleteReviewAsync(Guid reviewId)
        {
            try
            {
                await _reviewRepository.DeleteAsync(reviewId);
                _logger.LogInformation("Review admin deleted: {ReviewId}", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error admin deleting review {ReviewId}", reviewId);
                return false;
            }
        }

        public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
        {
            try
            {
                var allReviews = await _reviewRepository.GetAllAsync();
                var reviewsList = allReviews.ToList();

                if (!reviewsList.Any())
                {
                    return new ReviewStatisticsDto();
                }

                var ratingDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    ratingDistribution[i] = reviewsList.Count(r => r.Rating == i);
                }

                return new ReviewStatisticsDto
                {
                    TotalReviews = reviewsList.Count,
                    AverageRating = reviewsList.Average(r => r.Rating),
                    ReviewsThisMonth = reviewsList.Count(r => r.CreateDate >= DateTime.UtcNow.AddDays(-30)),
                    RatingDistribution = ratingDistribution,
                    MostActiveReviewers = await GetMostActiveReviewers(5)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics");
                return new ReviewStatisticsDto();
            }
        }

        private async Task<List<ClientReviewStatsDto>> GetMostActiveReviewers(int count)
        {
            try
            {
                var allReviews = await _reviewRepository.GetAllAsync();
                var reviewGroups = allReviews
                    .GroupBy(r => r.ClientId)
                    .OrderByDescending(g => g.Count())
                    .Take(count);

                var result = new List<ClientReviewStatsDto>();

                foreach (var group in reviewGroups)
                {
                    var client = await _clientRepository.GetByIdAsync(group.Key);
                    if (client != null)
                    {
                        result.Add(new ClientReviewStatsDto
                        {
                            ClientId = client.Id,
                            ClientName = client.FullName,
                            ReviewCount = group.Count(),
                            AverageRating = group.Average(r => r.Rating)
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most active reviewers");
                return new List<ClientReviewStatsDto>();
            }
        }
    }
}
