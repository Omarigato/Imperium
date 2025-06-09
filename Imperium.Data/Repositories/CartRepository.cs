using Microsoft.EntityFrameworkCore;
using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(c => c.Product).ThenInclude(p => p.Category)
                .Include(c => c.SelectedColor)
                .Include(c => c.SelectedSize)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Cart?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task ClearUserCartAsync(Guid userId)
        {
            var cartItems = await _dbSet.Where(c => c.UserId == userId).ToListAsync();
            _dbSet.RemoveRange(cartItems);
        }
    }
}