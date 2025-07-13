using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.Services.Dictionary
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IMapper _mapper;

        public DictionaryService(IDictionaryRepository dictionaryRepository, IMapper mapper)
        {
            _dictionaryRepository = dictionaryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DictionaryDto>> GetAllAsync()
        {
            var dictionaries = await _dictionaryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(dictionaries);
        }

        public async Task<IEnumerable<DictionaryDto>> GetByTypeAsync(string type)
        {
            var dictionaries = await _dictionaryRepository.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<DictionaryDto>>(dictionaries);
        }

        public async Task<DictionaryDto?> GetByIdAsync(Guid id)
        {
            var dictionary = await _dictionaryRepository.GetByIdAsync(id);
            return dictionary != null ? _mapper.Map<DictionaryDto>(dictionary) : null;
        }

        public async Task<DictionaryDto?> GetByCodeAsync(string code)
        {
            var dictionary = await _dictionaryRepository.GetByCodeAsync(code);
            return dictionary != null ? _mapper.Map<DictionaryDto>(dictionary) : null;
        }

        public async Task<IEnumerable<DictionaryDto>> GetCategoriesAsync()
        {
            var categories = await _dictionaryRepository.GetByTypeAsync(Core.Enums.DictionaryType.Categories);
            return _mapper.Map<IEnumerable<DictionaryDto>>(categories);
        }

        public async Task<IEnumerable<DictionaryDto>> GetColorsAsync()
        {
            var colors = await _dictionaryRepository.GetByTypeAsync(Core.Enums.DictionaryType.Colors);
            return _mapper.Map<IEnumerable<DictionaryDto>>(colors);
        }

        public async Task<IEnumerable<DictionaryDto>> GetSizesAsync()
        {
            var sizes = await _dictionaryRepository.GetByTypeAsync(Core.Enums.DictionaryType.Sizes);
            return _mapper.Map<IEnumerable<DictionaryDto>>(sizes);
        }

        public async Task<IEnumerable<DictionaryDto>> GetMaterialsAsync()
        {
            var materials = await _dictionaryRepository.GetByTypeAsync(Core.Enums.DictionaryType.Materials);
            return _mapper.Map<IEnumerable<DictionaryDto>>(materials);
        }
    }
}