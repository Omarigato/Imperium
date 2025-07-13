using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.OrderItem
{
    public interface IOrderItemRepository : IBaseRepository<Core.Models.OrderItem>
    {
        Task Insert(Core.Models.OrderItem orderItem);
        Task Update(Core.Models.OrderItem orderItem);
        Task<IEnumerable<Core.Models.OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Core.Models.OrderItem>> GetByProductIdAsync(Guid productId);
        Task<bool> DeleteByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Core.Models.OrderItem>> GetByOrderIdWithDetailsAsync(Guid orderId);
    }
}