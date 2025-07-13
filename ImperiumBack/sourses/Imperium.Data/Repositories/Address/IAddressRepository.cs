using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Address
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        Task<IEnumerable<Address>> GetByClientIdAsync(Guid clientId);
        Task<Address?> GetDefaultByClientIdAsync(Guid clientId);
        Task<bool> SetDefaultAddressAsync(Guid clientId, Guid addressId);
        Task<bool> UnsetDefaultAddressesAsync(Guid clientId);
    }
}