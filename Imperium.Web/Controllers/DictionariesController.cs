using Microsoft.AspNetCore.Mvc;
using Imperium.Service.Services;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionariesController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;

        public DictionariesController(IDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<DictionaryDto>>> GetCategories()
        {
            var categories = await _dictionaryService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("colors")]
        public async Task<ActionResult<IEnumerable<DictionaryDto>>> GetColors()
        {
            var colors = await _dictionaryService.GetColorsAsync();
            return Ok(colors);
        }

        [HttpGet("sizes")]
        public async Task<ActionResult<IEnumerable<DictionaryDto>>> GetSizes()
        {
            var sizes = await _dictionaryService.GetSizesAsync();
            return Ok(sizes);
        }

        [HttpGet("materials")]
        public async Task<ActionResult<IEnumerable<DictionaryDto>>> GetMaterials()
        {
            var materials = await _dictionaryService.GetMaterialsAsync();
            return Ok(materials);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<DictionaryDto>>> GetByType(string type)
        {
            var dictionaries = await _dictionaryService.GetByTypeAsync(type);
            return Ok(dictionaries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DictionaryDto>> GetById(Guid id)
        {
            var dictionary = await _dictionaryService.GetByIdAsync(id);
            if (dictionary == null)
                return NotFound();

            return Ok(dictionary);
        }
    }
}