using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Product
{
    public interface IProductRepository : IBaseRepository<Core.Models.Product>
    {
        Task Insert(Core.Models.Product product);
        Task Update(Core.Models.Product product);
        Task<IEnumerable<Core.Models.Product>> GetByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Core.Models.Product>> GetAvailableAsync();
        Task<Core.Models.Product?> GetByCodeAsync(string code);
        Task<IEnumerable<Core.Models.Product>> SearchAsync(string searchTerm);
        Task<Core.Models.Product?> GetWithDetailsAsync(Guid id);
        Task<IEnumerable<Core.Models.Product>> GetByCategoryWithDetailsAsync(Guid categoryId);
        Task<IEnumerable<Core.Models.Product>> GetByMaterialAsync(Guid materialId);
        Task<IEnumerable<Core.Models.Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Core.Models.Product>> GetByAuthorAsync(Guid authorId);
    }
}