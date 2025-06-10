using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IProductSizeRepository : IBaseRepository<ProductSize>
    {
        Task<IEnumerable<ProductSize>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductSize>> GetBySizeIdAsync(Guid sizeId);
        Task<bool> DeleteByProductAndSizeAsync(Guid productId, Guid sizeId);
        Task<bool> ExistsAsync(Guid productId, Guid sizeId);
        Task<IEnumerable<ProductSize>> GetAvailableByProductIdAsync(Guid productId);
    }
}