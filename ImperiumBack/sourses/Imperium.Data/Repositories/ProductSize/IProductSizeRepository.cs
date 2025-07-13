using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductSize
{
    public interface IProductSizeRepository : IBaseRepository<Core.Models.ProductSize>
    {
        Task Insert(Core.Models.ProductSize productSize);
        Task Update(Core.Models.ProductSize productSize);
        Task<IEnumerable<Core.Models.ProductSize>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.ProductSize>> GetBySizeIdAsync(Guid sizeId);
        Task<bool> DeleteByProductAndSizeAsync(Guid productId, Guid sizeId);
        Task<bool> ExistsAsync(Guid productId, Guid sizeId);
        Task<IEnumerable<Core.Models.ProductSize>> GetAvailableByProductIdAsync(Guid productId);
    }
}