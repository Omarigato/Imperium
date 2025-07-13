using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Address
{
    public interface IAddressRepository : IBaseRepository<Core.Models.Address>
    {
        Task Insert(Core.Models.Address address);
        Task Update(Core.Models.Address address);
        Task<IEnumerable<Core.Models.Address>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Address?> GetDefaultByClientIdAsync(Guid clientId);
        Task<bool> SetDefaultAddressAsync(Guid clientId, Guid addressId);
        Task<bool> UnsetDefaultAddressesAsync(Guid clientId);
    }
}