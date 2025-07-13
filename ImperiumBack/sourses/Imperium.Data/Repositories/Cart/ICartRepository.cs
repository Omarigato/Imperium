using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Cart
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
        Task Insert(Cart cart);
        Task Update(Cart cart);
        Task<IEnumerable<Cart>> GetByClientIdAsync(Guid clientId);
        Task<Cart?> GetByClientAndProductAsync(Guid clientId, Guid productId);
        Task<bool> ClearClientCartAsync(Guid clientId);
        Task<IEnumerable<Cart>> GetByClientIdWithDetailsAsync(Guid clientId);
    }
}