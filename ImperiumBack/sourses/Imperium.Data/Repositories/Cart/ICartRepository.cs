using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Cart
{
    public interface ICartRepository : IBaseRepository<Core.Models.Cart>
    {
        Task Insert(Core.Models.Cart cart);
        Task Update(Core.Models.Cart cart);
        Task<IEnumerable<Core.Models.Cart>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Cart?> GetByClientAndProductAsync(Guid clientId, Guid productId);
        Task<bool> ClearClientCartAsync(Guid clientId);
        Task<IEnumerable<Core.Models.Cart>> GetByClientIdWithDetailsAsync(Guid clientId);
    }
}