using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Product;

namespace Imperium.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _unitOfWork.Products.GetAvailableAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
        {
            var products = await _unitOfWork.Products.GetFeaturedAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto?> GetByCodeAsync(string code)
        {
            var product = await _unitOfWork.Products.GetByCodeAsync(code);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
        {
            var products = await _unitOfWork.Products.SearchAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            
            await _unitOfWork.Products.AddAsync(product);
            
            // Add colors
            foreach (var colorId in createProductDto.ColorIds)
            {
                var productColor = new ProductColor
                {
                    ProductId = product.Id,
                    ColorId = colorId
                };
                await _unitOfWork.Products.AddAsync(productColor);
            }
            
            // Add sizes
            foreach (var sizeId in createProductDto.SizeIds)
            {
                var productSize = new ProductSize
                {
                    ProductId = product.Id,
                    SizeId = sizeId
                };
                await _unitOfWork.Products.AddAsync(productSize);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            var createdProduct = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
            return _mapper.Map<ProductDto>(createdProduct!);
        }

        public async Task<ProductDto> UpdateAsync(Guid id, CreateProductDto updateProductDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            _mapper.Map(updateProductDto, product);
            product.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var updatedProduct = await _unitOfWork.Products.GetWithDetailsAsync(id);
            return _mapper.Map<ProductDto>(updatedProduct!);
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
