using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId);
        Task<Cart?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task ClearUserCartAsync(Guid userId);
    }
}