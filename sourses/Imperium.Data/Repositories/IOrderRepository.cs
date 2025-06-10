using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<Order?> GetWithDetailsAsync(Guid id);
        Task<string> GenerateOrderNumberAsync();
        Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(Guid userId);
        Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    }
}