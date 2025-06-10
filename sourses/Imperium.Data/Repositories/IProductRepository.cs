using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Product>> GetFeaturedAsync();
        Task<IEnumerable<Product>> GetAvailableAsync();
        Task<Product?> GetByCodeAsync(string code);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<Product?> GetWithDetailsAsync(Guid id);
    }
}