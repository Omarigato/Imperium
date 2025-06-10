using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
        Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId);
        Task<Cart?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<bool> ClearUserCartAsync(Guid userId);
        Task<IEnumerable<Cart>> GetByUserIdWithDetailsAsync(Guid userId);
    }
}