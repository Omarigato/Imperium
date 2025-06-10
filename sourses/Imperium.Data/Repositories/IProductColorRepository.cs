using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IProductColorRepository : IBaseRepository<ProductColor>
    {
        Task<IEnumerable<ProductColor>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductColor>> GetByColorIdAsync(Guid colorId);
        Task<bool> DeleteByProductAndColorAsync(Guid productId, Guid colorId);
        Task<bool> ExistsAsync(Guid productId, Guid colorId);
        Task<IEnumerable<ProductColor>> GetAvailableByProductIdAsync(Guid productId);
    }
}