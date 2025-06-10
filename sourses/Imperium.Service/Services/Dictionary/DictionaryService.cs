using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.Services.Dictionary
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DictionaryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DictionaryDto>> GetAllAsync()
        {
            var dictionaries = await _unitOfWork.Dictionaries.GetAllAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(dictionaries);
        }

        public async Task<IEnumerable<DictionaryDto>> GetByTypeAsync(string type)
        {
            var dictionaries = await _unitOfWork.Dictionaries.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<DictionaryDto>>(dictionaries);
        }

        public async Task<DictionaryDto?> GetByIdAsync(Guid id)
        {
            var dictionary = await _unitOfWork.Dictionaries.GetByIdAsync(id);
            return dictionary != null ? _mapper.Map<DictionaryDto>(dictionary) : null;
        }

        public async Task<DictionaryDto?> GetByCodeAsync(string code)
        {
            var dictionary = await _unitOfWork.Dictionaries.GetByCodeAsync(code);
            return dictionary != null ? _mapper.Map<DictionaryDto>(dictionary) : null;
        }

        public async Task<IEnumerable<DictionaryDto>> GetCategoriesAsync()
        {
            var categories = await _unitOfWork.Dictionaries.GetCategoriesAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(categories);
        }

        public async Task<IEnumerable<DictionaryDto>> GetColorsAsync()
        {
            var colors = await _unitOfWork.Dictionaries.GetColorsAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(colors);
        }

        public async Task<IEnumerable<DictionaryDto>> GetSizesAsync()
        {
            var sizes = await _unitOfWork.Dictionaries.GetSizesAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(sizes);
        }

        public async Task<IEnumerable<DictionaryDto>> GetMaterialsAsync()
        {
            var materials = await _unitOfWork.Dictionaries.GetMaterialsAsync();
            return _mapper.Map<IEnumerable<DictionaryDto>>(materials);
        }
    }
}