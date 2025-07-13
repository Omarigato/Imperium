using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Client
{
    public interface IClientRepository : IBaseRepository<Core.Models.Client>
    {
        Task Insert(Core.Models.Client client);
        Task Update(Core.Models.Client client);
        Task<Core.Models.Client?> GetByPhoneAsync(string phone);
        Task<bool> PhoneExistsAsync(string phone);
        Task<IEnumerable<Core.Models.Client>> GetActiveClientsAsync();
        Task<IEnumerable<Core.Models.Client>> GetVerifiedClientsAsync();
        Task<bool> VerifyPhoneAsync(Guid clientId);
    }
}