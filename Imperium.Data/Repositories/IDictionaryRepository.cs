using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public interface IDictionaryRepository : IGenericRepository<Dictionary>
    {
        Task<IEnumerable<Dictionary>> GetByTypeAsync(string type);
        Task<Dictionary?> GetByCodeAsync(string code);
        Task<IEnumerable<Dictionary>> GetCategoriesAsync();
        Task<IEnumerable<Dictionary>> GetColorsAsync();
        Task<IEnumerable<Dictionary>> GetSizesAsync();
        Task<IEnumerable<Dictionary>> GetMaterialsAsync();
    }
}