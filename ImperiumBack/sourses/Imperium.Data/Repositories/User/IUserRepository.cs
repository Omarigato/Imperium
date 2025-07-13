using Imperium.Data.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.User
{
    public interface IUserRepository : IBaseRepository<Core.Models.User>
    {
        Task Insert(Core.Models.User user);
        Task Update(Core.Models.User user);
        Task<Core.Models.User?> GetByLoginAsync(string login);
        Task<Core.Models.User?> GetByPhoneAsync(string phone);
        Task<bool> LoginExistsAsync(string login);
        Task<bool> PhoneExistsAsync(string phone);
        Task<IEnumerable<Core.Models.User>> GetActiveUsersAsync();
        Task<IEnumerable<Core.Models.User>> GetByRoleAsync(string role);
    }
}
