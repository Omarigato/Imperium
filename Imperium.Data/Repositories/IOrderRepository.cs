using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<Order?> GetWithDetailsAsync(Guid id);
        Task<string> GenerateOrderNumberAsync();
    }
}