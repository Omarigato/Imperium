using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductColor
{
    public interface IProductColorRepository : IBaseRepository<Core.Models.ProductColor>
    {
        Task Insert(Core.Models.ProductColor productColor);
        Task Update(Core.Models.ProductColor productColor);
        Task<IEnumerable<Core.Models.ProductColor>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.ProductColor>> GetByColorIdAsync(Guid colorId);
        Task<bool> DeleteByProductAndColorAsync(Guid productId, Guid colorId);
        Task<bool> ExistsAsync(Guid productId, Guid colorId);
        Task<IEnumerable<Core.Models.ProductColor>> GetAvailableByProductIdAsync(Guid productId);
    }
}