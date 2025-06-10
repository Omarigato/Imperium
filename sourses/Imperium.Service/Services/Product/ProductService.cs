using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Product;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAvailableAsync();
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
        {
            try
            {
                var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetFeaturedAsync();
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured products");
                throw;
            }
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
                return product != null ? _mapper.Map<ProductDto>(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by id {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto?> GetByCodeAsync(string code)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByCodeAsync(code);
                return product != null ? _mapper.Map<ProductDto>(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by code {Code}", code);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
        {
            try
            {
                var products = await _unitOfWork.Products.SearchAsync(searchTerm);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            try
            {
                var product = _mapper.Map<Core.Models.Product>(createProductDto);

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Добавляем цвета и размеры
                foreach (var colorId in createProductDto.ColorIds)
                {
                    var productColor = new ProductColor
                    {
                        ProductId = product.Id,
                        ColorId = colorId
                    };
                    await _unitOfWork.ProductColors.AddAsync(productColor);
                }

                foreach (var sizeId in createProductDto.SizeIds)
                {
                    var productSize = new ProductSize
                    {
                        ProductId = product.Id,
                        SizeId = sizeId
                    };
                    await _unitOfWork.ProductSizes.AddAsync(productSize);
                }

                await _unitOfWork.SaveChangesAsync();

                var createdProduct = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
                _logger.LogInformation("Product created successfully: {ProductId}", product.Id);

                return _mapper.Map<ProductDto>(createdProduct!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductDto> UpdateAsync(Guid id, CreateProductDto updateProductDto)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    throw new KeyNotFoundException("Product not found");

                // Обновляем основные поля
                product.CategoryId = updateProductDto.CategoryId;
                product.MaterialId = updateProductDto.MaterialId;
                product.NameRu = updateProductDto.NameRu;
                product.NameKz = updateProductDto.NameKz;
                product.Code = updateProductDto.Code;
                product.DescriptionRu = updateProductDto.DescriptionRu;
                product.DescriptionKz = updateProductDto.DescriptionKz;
                product.Price = updateProductDto.Price;
                product.IsAvailable = updateProductDto.IsAvailable;
                product.IsFeatured = updateProductDto.IsFeatured;
                product.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                var updatedProduct = await _unitOfWork.Products.GetWithDetailsAsync(id);
                _logger.LogInformation("Product updated successfully: {ProductId}", id);

                return _mapper.Map<ProductDto>(updatedProduct!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    throw new KeyNotFoundException("Product not found");

                _unitOfWork.Products.Remove(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                throw;
            }
        }
    }

}
