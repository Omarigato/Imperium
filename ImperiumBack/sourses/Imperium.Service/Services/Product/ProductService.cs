using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Product;
using Imperium.Data.Repositories.ProductColor;
using Imperium.Data.Repositories.ProductFile;
using Imperium.Data.Repositories.ProductSize;
using Imperium.Service.DTOs.Dictionary;
using Imperium.Service.DTOs.Product;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IProductFileRepository _productFileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IDictionaryRepository dictionaryRepository,
            IFileRepository fileRepository,
            IProductColorRepository productColorRepository,
            IProductSizeRepository productSizeRepository,
            IProductFileRepository productFileRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _dictionaryRepository = dictionaryRepository;
            _fileRepository = fileRepository;
            _productColorRepository = productColorRepository;
            _productSizeRepository = productSizeRepository;
            _productFileRepository = productFileRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetAvailableAsync()
        {
            try
            {
                var products = await _productRepository.GetAvailableAsync();
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available products");
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(categoryId);
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
        {
            try
            {
                // Пока возвращаем первые 8 доступных товаров
                // TODO: Добавить поле IsFeatured в модель Product
                var products = await _productRepository.GetAvailableAsync();
                var featuredProducts = products.Take(8);
                var result = new List<ProductDto>();

                foreach (var product in featuredProducts)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return new List<ProductDto>();
            }
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var product = await _productRepository.GetWithDetailsAsync(id);
                if (product == null) return null;

                return await BuildProductDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return null;
            }
        }

        public async Task<ProductDto?> GetByCodeAsync(string code)
        {
            try
            {
                var product = await _productRepository.GetByCodeAsync(code);
                if (product == null) return null;

                return await BuildProductDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by code {Code}", code);
                return null;
            }
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
        {
            try
            {
                var products = await _productRepository.SearchAsync(searchTerm);
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term {SearchTerm}", searchTerm);
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderBy(p => p.Price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetByMaterialAsync(Guid materialId)
        {
            try
            {
                var products = await _productRepository.GetByMaterialAsync(materialId);
                var result = new List<ProductDto>();

                foreach (var product in products)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by material {MaterialId}", materialId);
                return new List<ProductDto>();
            }
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            try
            {
                // Проверяем уникальность кода
                var existingProduct = await _productRepository.GetByCodeAsync(createProductDto.Code);
                if (existingProduct != null)
                {
                    throw new InvalidOperationException("Товар с таким кодом уже существует");
                }

                // Проверяем существование категории
                var category = await _dictionaryRepository.GetByIdAsync(createProductDto.CategoryId);
                if (category == null)
                {
                    throw new InvalidOperationException("Категория не найдена");
                }

                // Проверяем материал, если указан
                if (createProductDto.MaterialId.HasValue)
                {
                    var material = await _dictionaryRepository.GetByIdAsync(createProductDto.MaterialId.Value);
                    if (material == null)
                    {
                        throw new InvalidOperationException("Материал не найден");
                    }
                }

                var product = new Core.Models.Product
                {
                    CategoryId = createProductDto.CategoryId,
                    MaterialId = createProductDto.MaterialId,
                    NameRu = createProductDto.NameRu,
                    NameKz = createProductDto.NameKz,
                    Code = createProductDto.Code,
                    DescriptionRu = createProductDto.DescriptionRu,
                    DescriptionKz = createProductDto.DescriptionKz,
                    Price = createProductDto.Price,
                    IsAvailable = createProductDto.IsAvailable,
                    AuthorId = createProductDto.AuthorId,
                    CreateDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _productRepository.Insert(product);

                // Добавляем цвета
                if (createProductDto.ColorIds?.Any() == true)
                {
                    await AddProductColorsAsync(product.Id, createProductDto.ColorIds, createProductDto.AuthorId);
                }

                // Добавляем размеры
                if (createProductDto.SizeIds?.Any() == true)
                {
                    await AddProductSizesAsync(product.Id, createProductDto.SizeIds, createProductDto.AuthorId);
                }

                _logger.LogInformation("Product created: {ProductCode} by {AuthorId}", product.Code, createProductDto.AuthorId);

                return await BuildProductDto(product) ?? throw new InvalidOperationException("Ошибка при создании товара");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductCode}", createProductDto.Code);
                throw;
            }
        }

        public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return null;

                // Проверяем уникальность кода, если он изменился
                if (product.Code != updateProductDto.Code)
                {
                    var existingProduct = await _productRepository.GetByCodeAsync(updateProductDto.Code);
                    if (existingProduct != null)
                    {
                        throw new InvalidOperationException("Товар с таким кодом уже существует");
                    }
                }

                // Обновляем поля
                product.CategoryId = updateProductDto.CategoryId;
                product.MaterialId = updateProductDto.MaterialId;
                product.NameRu = updateProductDto.NameRu;
                product.NameKz = updateProductDto.NameKz;
                product.Code = updateProductDto.Code;
                product.DescriptionRu = updateProductDto.DescriptionRu;
                product.DescriptionKz = updateProductDto.DescriptionKz;
                product.Price = updateProductDto.Price;
                product.IsAvailable = updateProductDto.IsAvailable;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.Update(product);

                _logger.LogInformation("Product updated: {ProductId}", id);

                return await BuildProductDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return false;

                // Мягкое удаление
                product.DeleteDate = DateTime.UtcNow;
                await _productRepository.Update(product);

                _logger.LogInformation("Product soft deleted: {ProductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return false;
            }
        }

        public async Task<bool> ToggleAvailabilityAsync(Guid id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return false;

                product.IsAvailable = !product.IsAvailable;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.Update(product);

                _logger.LogInformation("Product availability toggled: {ProductId} -> {IsAvailable}", id, product.IsAvailable);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product availability {ProductId}", id);
                return false;
            }
        }

        public async Task<bool> AddProductImagesAsync(Guid productId, List<Guid> fileIds, Guid authorId)
        {
            try
            {
                foreach (var fileId in fileIds)
                {
                    var file = await _fileRepository.GetByIdAsync(fileId);
                    if (file == null) continue;

                    var productFile = new ProductFile
                    {
                        ProductId = productId,
                        FileId = fileId,
                        IsAddition = false, // Главное изображение
                        AuthorId = authorId,
                        CreateDate = DateTime.UtcNow
                    };

                    await _productFileRepository.Insert(productFile);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding images to product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> RemoveProductImageAsync(Guid productId, Guid fileId)
        {
            try
            {
                return await _productFileRepository.DeleteByProductAndFileAsync(productId, fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing image from product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> SetMainImageAsync(Guid productId, Guid fileId)
        {
            try
            {
                // Сначала делаем все изображения дополнительными
                var productFiles = await _productFileRepository.GetByProductIdAsync(productId);
                foreach (var pf in productFiles)
                {
                    pf.IsAddition = true;
                    await _productFileRepository.Update(pf);
                }

                // Затем делаем выбранное изображение основным
                var mainProductFile = productFiles.FirstOrDefault(pf => pf.FileId == fileId);
                if (mainProductFile != null)
                {
                    mainProductFile.IsAddition = false;
                    await _productFileRepository.Update(mainProductFile);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting main image for product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> AddProductColorsAsync(Guid productId, List<Guid> colorIds, Guid authorId)
        {
            try
            {
                foreach (var colorId in colorIds)
                {
                    var exists = await _productColorRepository.ExistsAsync(productId, colorId);
                    if (!exists)
                    {
                        var productColor = new ProductColor
                        {
                            ProductId = productId,
                            ColorId = colorId,
                            IsAvailable = true,
                            AuthorId = authorId,
                            CreateDate = DateTime.UtcNow
                        };

                        await _productColorRepository.Insert(productColor);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding colors to product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> RemoveProductColorAsync(Guid productId, Guid colorId)
        {
            try
            {
                return await _productColorRepository.DeleteByProductAndColorAsync(productId, colorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing color from product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> AddProductSizesAsync(Guid productId, List<Guid> sizeIds, Guid authorId)
        {
            try
            {
                foreach (var sizeId in sizeIds)
                {
                    var exists = await _productSizeRepository.ExistsAsync(productId, sizeId);
                    if (!exists)
                    {
                        var productSize = new ProductSize
                        {
                            ProductId = productId,
                            SizeId = sizeId,
                            IsAvailable = true,
                            AuthorId = authorId,
                            CreateDate = DateTime.UtcNow
                        };

                        await _productSizeRepository.Insert(productSize);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sizes to product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> RemoveProductSizeAsync(Guid productId, Guid sizeId)
        {
            try
            {
                return await _productSizeRepository.DeleteByProductAndSizeAsync(productId, sizeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing size from product {ProductId}", productId);
                return false;
            }
        }

        public async Task<ProductStatisticsDto> GetProductStatisticsAsync()
        {
            try
            {
                var allProducts = await _productRepository.GetAllAsync();
                var availableProducts = allProducts.Where(p => p.IsAvailable);

                return new ProductStatisticsDto
                {
                    TotalProducts = allProducts.Count(),
                    AvailableProducts = availableProducts.Count(),
                    OutOfStockProducts = allProducts.Count(p => !p.IsAvailable),
                    ProductsCreatedThisMonth = allProducts.Count(p => p.CreateDate >= DateTime.UtcNow.AddDays(-30)),
                    AveragePrice = availableProducts.Any() ? availableProducts.Average(p => p.Price) : 0,
                    MinPrice = availableProducts.Any() ? availableProducts.Min(p => p.Price) : 0,
                    MaxPrice = availableProducts.Any() ? availableProducts.Max(p => p.Price) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product statistics");
                return new ProductStatisticsDto();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetTopSellingProductsAsync(int count = 10)
        {
            try
            {
                // TODO: Implement based on order statistics
                // Пока возвращаем первые доступные товары
                var products = await _productRepository.GetAvailableAsync();
                var topProducts = products.Take(count);
                var result = new List<ProductDto>();

                foreach (var product in topProducts)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                return new List<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            try
            {
                // TODO: Implement based on stock levels when stock management is added
                // Пока возвращаем недоступные товары
                var allProducts = await _productRepository.GetAllAsync();
                var lowStockProducts = allProducts.Where(p => !p.IsAvailable);
                var result = new List<ProductDto>();

                foreach (var product in lowStockProducts)
                {
                    var productDto = await BuildProductDto(product);
                    if (productDto != null)
                    {
                        result.Add(productDto);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                return new List<ProductDto>();
            }
        }

        private async Task<ProductDto?> BuildProductDto(Core.Models.Product product)
        {
            try
            {
                // Получаем категорию
                var category = await _dictionaryRepository.GetByIdAsync(product.CategoryId);

                // Получаем материал
                Core.Models.Dictionary? material = null;
                if (product.MaterialId.HasValue)
                {
                    material = await _dictionaryRepository.GetByIdAsync(product.MaterialId.Value);
                }

                // Получаем доступные цвета
                var productColors = await _productColorRepository.GetAvailableByProductIdAsync(product.Id);
                var availableColors = new List<DictionaryDto>();
                foreach (var pc in productColors)
                {
                    var color = await _dictionaryRepository.GetByIdAsync(pc.ColorId);
                    if (color != null)
                    {
                        availableColors.Add(_mapper.Map<DictionaryDto>(color));
                    }
                }

                // Получаем доступные размеры
                var productSizes = await _productSizeRepository.GetAvailableByProductIdAsync(product.Id);
                var availableSizes = new List<DictionaryDto>();
                foreach (var ps in productSizes)
                {
                    var size = await _dictionaryRepository.GetByIdAsync(ps.SizeId);
                    if (size != null)
                    {
                        availableSizes.Add(_mapper.Map<DictionaryDto>(size));
                    }
                }

                // Получаем изображения
                var productFiles = await _productFileRepository.GetByProductIdAsync(product.Id);
                var images = new List<string>();
                foreach (var pf in productFiles)
                {
                    var file = await _fileRepository.GetByIdAsync(pf.FileId);
                    if (file != null)
                    {
                        images.Add(file.Url);
                    }
                }

                return new ProductDto
                {
                    Id = product.Id,
                    NameRu = product.NameRu,
                    NameKz = product.NameKz,
                    Code = product.Code,
                    DescriptionRu = product.DescriptionRu,
                    DescriptionKz = product.DescriptionKz,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    CreatedAt = product.CreateDate,
                    Category = category != null ? _mapper.Map<DictionaryDto>(category) : new DictionaryDto(),
                    Material = material != null ? _mapper.Map<DictionaryDto>(material) : null,
                    AvailableColors = availableColors,
                    AvailableSizes = availableSizes,
                    Images = images
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building ProductDto for product {ProductId}", product.Id);
                return null;
            }
        }
    }
}