using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Favorite
{
    public interface IFavoriteRepository : IBaseRepository<Core.Models.Favorite>
    {
        Task Insert(Core.Models.Favorite favorite);
        Task Update(Core.Models.Favorite favorite);
        Task<IEnumerable<Core.Models.Favorite>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Favorite?> GetByClientAndProductAsync(Guid clientId, Guid productId);
        Task<bool> ExistsAsync(Guid clientId, Guid productId);
        Task<bool> RemoveByClientAndProductAsync(Guid clientId, Guid productId);
        Task<IEnumerable<Core.Models.Favorite>> GetByClientIdWithDetailsAsync(Guid clientId);
    }
}