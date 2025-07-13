using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Dictionary
{
    public interface IDictionaryRepository : IBaseRepository<Core.Models.Dictionary>
    {
        Task Insert(Core.Models.Dictionary dictionary);
        Task Update(Core.Models.Dictionary dictionary);
        Task<IEnumerable<Core.Models.Dictionary>> GetByTypeAsync(string type);
        Task<Core.Models.Dictionary?> GetByCodeAsync(string code);
        Task<IEnumerable<Core.Models.Dictionary>> GetCategoriesWithChildrenAsync();
        Task<IEnumerable<Core.Models.Dictionary>> GetChildrenByParentIdAsync(Guid parentId);
        Task<bool> DeactivateAsync(Guid id);
    }
}