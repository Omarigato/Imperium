using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imperium.Service.DTOs.Product;

namespace Imperium.Service.Services.Product
{
    public interface IProductService
    {
        // Публичные методы (для клиентов)
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetAvailableAsync();
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductDto>> GetFeaturedAsync();
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto?> GetByCodeAsync(string code);
        Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm);
        Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductDto>> GetByMaterialAsync(Guid materialId);

        // Админские методы
        Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ToggleAvailabilityAsync(Guid id);

        // Управление изображениями
        Task<bool> AddProductImagesAsync(Guid productId, List<Guid> fileIds, Guid authorId);
        Task<bool> RemoveProductImageAsync(Guid productId, Guid fileId);
        Task<bool> SetMainImageAsync(Guid productId, Guid fileId);

        // Управление цветами и размерами
        Task<bool> AddProductColorsAsync(Guid productId, List<Guid> colorIds, Guid authorId);
        Task<bool> RemoveProductColorAsync(Guid productId, Guid colorId);
        Task<bool> AddProductSizesAsync(Guid productId, List<Guid> sizeIds, Guid authorId);
        Task<bool> RemoveProductSizeAsync(Guid productId, Guid sizeId);

        // Статистика
        Task<ProductStatisticsDto> GetProductStatisticsAsync();
        Task<IEnumerable<ProductDto>> GetTopSellingProductsAsync(int count = 10);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    }
}