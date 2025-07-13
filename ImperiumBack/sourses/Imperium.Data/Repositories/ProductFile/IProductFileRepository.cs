using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductFile
{
    public interface IProductFileRepository : IBaseRepository<Core.Models.ProductFile>
    {
        Task Insert(Core.Models.ProductFile productFile);
        Task Update(Core.Models.ProductFile productFile);
        Task<IEnumerable<Core.Models.ProductFile>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.ProductFile>> GetByFileIdAsync(Guid fileId);
        Task<bool> DeleteByProductAndFileAsync(Guid productId, Guid fileId);
        Task<bool> ExistsAsync(Guid productId, Guid fileId);
        Task<IEnumerable<Core.Models.ProductFile>> GetMainImagesByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.ProductFile>> GetAdditionalImagesByProductIdAsync(Guid productId);
    }
}