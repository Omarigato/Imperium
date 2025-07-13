using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Data.Repositories.Product;
using Imperium.Data.Repositories.ProductColor;
using Imperium.Data.Repositories.ProductSize;
using Imperium.Service.DTOs.Product;

namespace Imperium.Service.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IDictionaryRepository dictionaryRepository,
            IProductColorRepository productColorRepository,
            IProductSizeRepository productSizeRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _dictionaryRepository = dictionaryRepository;
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

            // Добавляем связи с цветами
            foreach (var colorId in createProductDto.ColorIds)
            {
                await _productColorRepository.AddAsync(new ProductColor
                {
                    ProductId = productId,
                    ColorId = colorId,
                    IsAvailable = true
                });
            }

            // Добавляем связи с размерами
            foreach (var sizeId in createProductDto.SizeIds)
            {
                await _productSizeRepository.AddAsync(new ProductSize
                {
                    ProductId = productId,
                    SizeId = sizeId,
                    IsAvailable = true
                });
            }

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateAsync(Guid id, CreateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            _mapper.Map(updateProductDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return _mapper.Map<ProductDto>(product);
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