using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imperium.Service.DTOs.Product;

namespace Imperium.Service.Services.Product
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductDto>> GetFeaturedAsync();
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto?> GetByCodeAsync(string code);
        Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm);
        Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateAsync(Guid id, CreateProductDto updateProductDto);
        Task DeleteAsync(Guid id);
    }
}