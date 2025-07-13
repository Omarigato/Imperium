using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.File
{
    public interface IFileRepository : IBaseRepository<Core.Models.File>
    {
        Task Insert(Core.Models.File file);
        Task Update(Core.Models.File file);
        Task<Core.Models.File?> GetByPublicIdAsync(string publicId);
        Task<IEnumerable<Core.Models.File>> GetByProductIdAsync(Guid productId);
        Task<bool> DeleteByPublicIdAsync(string publicId);
        Task<IEnumerable<Core.Models.File>> GetByAuthorAsync(Guid authorId);
        Task<IEnumerable<Core.Models.File>> GetByMimeTypeAsync(string mimeType);
    }
}