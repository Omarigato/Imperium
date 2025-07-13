using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Verification
{
    public interface IVerificationRepository : IBaseRepository<Core.Models.Verification>
    {
        Task Insert(Core.Models.Verification verification);
        Task Update(Core.Models.Verification verification);
        Task<IEnumerable<Core.Models.Verification>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Verification?> GetActiveByClientAndContactAsync(Guid clientId, string contact, string type);
        Task<bool> DeleteExpiredAsync();
        Task<IEnumerable<Core.Models.Verification>> GetByContactAndTypeAsync(string contact, string type);
        Task<bool> MarkAsUsedAsync(Guid verificationId);
    }
}