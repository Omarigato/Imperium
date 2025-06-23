using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IPasswordResetRepository : IBaseRepository<PasswordReset>
    {
        Task<PasswordReset?> GetActiveByTokenAsync(string resetToken);
        Task<PasswordReset?> GetActiveByEmailAsync(string email);
        Task<bool> MarkAsUsedAsync(Guid id);
        Task<bool> DeleteExpiredAsync();
    }
}
