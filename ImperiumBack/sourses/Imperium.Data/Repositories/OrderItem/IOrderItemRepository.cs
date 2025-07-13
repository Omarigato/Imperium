using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId);
        Task<bool> DeleteByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetByOrderIdWithDetailsAsync(Guid orderId);
    }
}