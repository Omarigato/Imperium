using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Product
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Product>> GetFeaturedAsync();
        Task<IEnumerable<Product>> GetAvailableAsync();
        Task<Product?> GetByCodeAsync(string code);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<Product?> GetWithDetailsAsync(Guid id);
        Task<IEnumerable<Product>> GetByCategoryWithDetailsAsync(Guid categoryId);
    }
}