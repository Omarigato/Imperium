using Microsoft.EntityFrameworkCore;
using Imperium.Core.Models;
using Imperium.Core.Enums;

namespace Imperium.Data.Repositories
{
    public class DictionaryRepository : GenericRepository<Dictionary>, IDictionaryRepository
    {
        public DictionaryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Dictionary>> GetByTypeAsync(string type)
        {
            return await _dbSet
                .Where(d => d.Type == type && d.IsActive)
                .OrderBy(d => d.NameRu)
                .ToListAsync();
        }

        public async Task<Dictionary?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.Code == code && d.IsActive);
        }

        public async Task<IEnumerable<Dictionary>> GetCategoriesAsync()
        {
            return await GetByTypeAsync(DictionaryType.Categories);
        }

        public async Task<IEnumerable<Dictionary>> GetColorsAsync()
        {
            return await GetByTypeAsync(DictionaryType.Colors);
        }

        public async Task<IEnumerable<Dictionary>> GetSizesAsync()
        {
            return await GetByTypeAsync(DictionaryType.Sizes);
        }

        public async Task<IEnumerable<Dictionary>> GetMaterialsAsync()
        {
            return await GetByTypeAsync(DictionaryType.Materials);
        }
    }
}