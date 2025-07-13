using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Order
{
    public interface IOrderRepository : IBaseRepository<Core.Models.Order>
    {
        Task Insert(Core.Models.Order order);
        Task Update(Core.Models.Order order);
        Task<IEnumerable<Core.Models.Order>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Order?> GetByOrderNumberAsync(string orderNumber);
        Task<Core.Models.Order?> GetWithDetailsAsync(Guid id);
        Task<string> GenerateOrderNumberAsync();
        Task<IEnumerable<Core.Models.Order>> GetByClientIdWithDetailsAsync(Guid clientId);
        Task<IEnumerable<Core.Models.Order>> GetAllWithDetailsAsync();
        Task<IEnumerable<Core.Models.Order>> GetByStatusAsync(string status);
        Task<bool> UpdateStatusAsync(Guid orderId, string status);
    }
}