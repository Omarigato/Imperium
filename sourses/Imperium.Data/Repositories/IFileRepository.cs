using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IFileRepository : IBaseRepository<File>
    {
        Task<File?> GetByPublicIdAsync(string publicId);
        Task<IEnumerable<File>> GetByProductIdAsync(Guid productId);
        Task<bool> DeleteByPublicIdAsync(string publicId);
    }
}