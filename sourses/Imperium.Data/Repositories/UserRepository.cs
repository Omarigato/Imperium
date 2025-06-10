using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone && u.DeletedAt == null);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _dbSet.AnyAsync(u => u.Phone == phone && u.DeletedAt == null);
        }
    }
}