using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IFavoriteRepository : IBaseRepository<Favorite>
    {
        Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId);
        Task<Favorite?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<bool> ExistsAsync(Guid userId, Guid productId);
        Task<bool> RemoveByUserAndProductAsync(Guid userId, Guid productId);
        Task<IEnumerable<Favorite>> GetByUserIdWithDetailsAsync(Guid userId);
    }
}