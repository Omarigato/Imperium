using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IProductFileRepository : IBaseRepository<ProductFile>
    {
        Task<IEnumerable<ProductFile>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductFile>> GetByFileIdAsync(Guid fileId);
        Task<bool> DeleteByProductAndFileAsync(Guid productId, Guid fileId);
        Task<bool> ExistsAsync(Guid productId, Guid fileId);
    }
}