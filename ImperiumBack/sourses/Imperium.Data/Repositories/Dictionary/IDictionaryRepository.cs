using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Dictionary
{
    public interface IDictionaryRepository : IBaseRepository<Dictionary>
    {
        Task<IEnumerable<Dictionary>> GetByTypeAsync(string type);
        Task<Dictionary?> GetByCodeAsync(string code);
        Task<IEnumerable<Dictionary>> GetCategoriesAsync();
        Task<IEnumerable<Dictionary>> GetColorsAsync();
        Task<IEnumerable<Dictionary>> GetSizesAsync();
        Task<IEnumerable<Dictionary>> GetMaterialsAsync();
        Task<IEnumerable<Dictionary>> GetCategoriesWithChildrenAsync();
    }
}