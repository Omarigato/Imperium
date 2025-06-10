using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId);
        Task<Address?> GetDefaultByUserIdAsync(Guid userId);
        Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId);
        Task<bool> UnsetDefaultAddressesAsync(Guid userId);
    }
}