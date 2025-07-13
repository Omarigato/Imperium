using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Verification
{
    public interface IVerificationRepository : IBaseRepository<Verification>
    {
        Task<IEnumerable<Verification>> GetByUserIdAsync(Guid userId);
        Task<Verification?> GetActiveByUserAndContactAsync(Guid userId, string contact, string type);
        Task<bool> DeleteExpiredAsync();
        Task<IEnumerable<Verification>> GetByContactAndTypeAsync(string contact, string type);
    }
}