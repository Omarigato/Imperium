using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.Services
{
    public interface IDictionaryService
    {
        Task<IEnumerable<DictionaryDto>> GetAllAsync();
        Task<IEnumerable<DictionaryDto>> GetByTypeAsync(string type);
        Task<DictionaryDto?> GetByIdAsync(Guid id);
        Task<DictionaryDto?> GetByCodeAsync(string code);
        Task<IEnumerable<DictionaryDto>> GetCategoriesAsync();
        Task<IEnumerable<DictionaryDto>> GetColorsAsync();
        Task<IEnumerable<DictionaryDto>> GetSizesAsync();
        Task<IEnumerable<DictionaryDto>> GetMaterialsAsync();
    }
}