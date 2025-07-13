using AutoMapper;
using Imperium.Data.Repositories.Favorite;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Product;
using Imperium.Service.DTOs.Favorite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Favorite
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            IProductRepository productRepository,
            IFileRepository fileRepository,
            IMapper mapper,
            ILogger<FavoriteService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<FavoriteDto>> GetClientFavoritesAsync(Guid clientId)
        {
            try
            {
                var favorites = await _favoriteRepository.GetByClientIdWithDetailsAsync(clientId);
                var result = new List<FavoriteDto>();

                foreach (var favorite in favorites)
                {
                    var product = await _productRepository.GetWithDetailsAsync(favorite.ProductId);
                    if (product != null && product.IsAvailable)
                    {
                        var productImages = await _fileRepository.GetByProductIdAsync(product.Id);

                        result.Add(new FavoriteDto
                        {
                            Id = favorite.Id,
                            ProductId = product.Id,
                            CreateDate = favorite.CreateDate,
                            ProductName = product.NameRu,
                            ProductCode = product.Code,
                            Price = product.Price,
                            IsAvailable = product.IsAvailable,
                            ProductImages = productImages.Select(f => f.Url).ToList(),
                            CategoryName = product.Category?.NameRu ?? ""
                        });
                    }
                }

                return result.OrderByDescending(f => f.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites for client {ClientId}", clientId);
                return new List<FavoriteDto>();
            }
        }

        public async Task<FavoriteDto?> AddToFavoriteAsync(Guid clientId, AddToFavoriteDto addDto)
        {
            try
            {
                // Проверяем, существует ли товар
                var product = await _productRepository.GetWithDetailsAsync(addDto.ProductId);
                if (product == null || !product.IsAvailable)
                {
                    throw new InvalidOperationException("Товар не найден или недоступен");
                }

                // Проверяем, нет ли уже в избранном
                var existingFavorite = await _favoriteRepository.GetByClientAndProductAsync(clientId, addDto.ProductId);
                if (existingFavorite != null)
                {
                    throw new InvalidOperationException("Товар уже добавлен в избранное");
                }

                // Добавляем в избранное
                var favorite = new Core.Models.Favorite
                {
                    ClientId = clientId,
                    ProductId = addDto.ProductId,
                    CreateDate = DateTime.UtcNow
                };

                await _favoriteRepository.Insert(favorite);

                var productImages = await _fileRepository.GetByProductIdAsync(product.Id);

                return new FavoriteDto
                {
                    Id = favorite.Id,
                    ProductId = product.Id,
                    CreateDate = favorite.CreateDate,
                    ProductName = product.NameRu,
                    ProductCode = product.Code,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    ProductImages = productImages.Select(f => f.Url).ToList(),
                    CategoryName = product.Category?.NameRu ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to favorites for client {ClientId}",
                    addDto.ProductId, clientId);
                throw;
            }
        }

        public async Task<bool> RemoveFromFavoriteAsync(Guid clientId, Guid productId)
        {
            try
            {
                return await _favoriteRepository.RemoveByClientAndProductAsync(clientId, productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from favorites for client {ClientId}",
                    productId, clientId);
                return false;
            }
        }

        public async Task<bool> IsProductInFavoritesAsync(Guid clientId, Guid productId)
        {
            try
            {
                return await _favoriteRepository.ExistsAsync(clientId, productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} is in favorites for client {ClientId}",
                    productId, clientId);
                return false;
            }
        }

        public async Task<int> GetFavoriteCountAsync(Guid clientId)
        {
            try
            {
                var favorites = await _favoriteRepository.GetByClientIdAsync(clientId);
                return favorites.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite count for client {ClientId}", clientId);
                return 0;
            }
        }

        public async Task<bool> ClearFavoritesAsync(Guid clientId)
        {
            try
            {
                var favorites = await _favoriteRepository.GetByClientIdAsync(clientId);

                foreach (var favorite in favorites)
                {
                    await _favoriteRepository.DeleteAsync(favorite.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing favorites for client {ClientId}", clientId);
                return false;
            }
        }
    }
}
