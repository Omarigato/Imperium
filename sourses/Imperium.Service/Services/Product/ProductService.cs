using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Service.DTOs.Product;

namespace Imperium.Service.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IProductColorRepository productColorRepository,
            IProductSizeRepository productSizeRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _productColorRepository = productColorRepository;
            _productSizeRepository = productSizeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAvailableAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
        {
            var products = await _productRepository.GetFeaturedAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetWithDetailsAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto?> GetByCodeAsync(string code)
        {
            var product = await _productRepository.GetByCodeAsync(code);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
        {
            var products = await _productRepository.SearchAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Core.Models.Product>(createProductDto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            var productId = await _productRepository.AddAsync(product);
            product.Id = productId;

            // Добавляем цвета
            foreach (var colorId in createProductDto.ColorIds)
            {
                var productColor = new ProductColor
                {
                    ProductId = productId,
                    ColorId = colorId,
                    IsAvailable = true
                };
                await _productColorRepository.AddAsync(productColor);
            }

            // Добавляем размеры
            foreach (var sizeId in createProductDto.SizeIds)
            {
                var productSize = new ProductSize
                {
                    ProductId = productId,
                    SizeId = sizeId,
                    IsAvailable = true
                };
                await _productSizeRepository.AddAsync(productSize);
            }

            var createdProduct = await _productRepository.GetWithDetailsAsync(productId);
            return _mapper.Map<ProductDto>(createdProduct!);
        }

        public async Task<ProductDto> UpdateAsync(Guid id, CreateProductDto updateProductDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException("Product not found");

            _mapper.Map(updateProductDto, existingProduct);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existingProduct);

            // Обновляем цвета (простое решение - удаляем все и добавляем заново)
            var existingColors = await _productColorRepository.GetByProductIdAsync(id);
            foreach (var color in existingColors)
            {
                await _productColorRepository.DeleteAsync(color.Id);
            }

            foreach (var colorId in updateProductDto.ColorIds)
            {
                var productColor = new ProductColor
                {
                    ProductId = id,
                    ColorId = colorId,
                    IsAvailable = true
                };
                await _productColorRepository.AddAsync(productColor);
            }

            // Обновляем размеры
            var existingSizes = await _productSizeRepository.GetByProductIdAsync(id);
            foreach (var size in existingSizes)
            {
                await _productSizeRepository.DeleteAsync(size.Id);
            }

            foreach (var sizeId in updateProductDto.SizeIds)
            {
                var productSize = new ProductSize
                {
                    ProductId = id,
                    SizeId = sizeId,
                    IsAvailable = true
                };
                await _productSizeRepository.AddAsync(productSize);
            }

            var updatedProduct = await _productRepository.GetWithDetailsAsync(id);
            return _mapper.Map<ProductDto>(updatedProduct!);
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            await _productRepository.DeleteAsync(id);
        }
    }
}