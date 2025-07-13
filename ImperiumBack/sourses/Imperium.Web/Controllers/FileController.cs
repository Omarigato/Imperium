using Imperium.Service.DTOs;
using Imperium.Service.DTOs.File;
using Imperium.Service.Services.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с файлами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Загрузить несколько файлов
        /// </summary>
        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<List<FileDto>>>> UploadMultipleFiles(
            [FromForm] List<IFormFile> files,
            [FromForm] string folder = "imperium")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest(new ApiResponse<List<FileDto>>
                    {
                        Success = false,
                        Message = "Файлы не выбраны для загрузки"
                    });
                }

                var authorId = GetCurrentUserId();
                var result = await _fileService.UploadFilesAsync(files, authorId, folder);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadMultipleFiles");
                return StatusCode(500, new ApiResponse<List<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при загрузке файлов"
                });
            }
        }

        /// <summary>
        /// Загрузить один файл
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<FileDto>>> UploadFile(
            [FromForm] IFormFile file,
            [FromForm] string folder = "imperium")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<FileDto>
                    {
                        Success = false,
                        Message = "Файл не выбран для загрузки"
                    });
                }

                var authorId = GetCurrentUserId();
                var result = await _fileService.UploadFileAsync(file, authorId, folder);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadFile");
                return StatusCode(500, new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Ошибка при загрузке файла"
                });
            }
        }

        /// <summary>
        /// Прикрепить файлы к продукту
        /// </summary>
        [HttpPost("attach-to-product")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> AttachFilesToProduct([FromBody] AttachFilesToProductDto attachDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Неверные данные запроса"
                    });
                }

                var authorId = GetCurrentUserId();
                var result = await _fileService.AttachFilesToProductAsync(
                    attachDto.ProductId,
                    attachDto.FileIds,
                    authorId,
                    attachDto.IsAddition);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching files to product {ProductId}", attachDto?.ProductId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при прикреплении файлов к продукту"
                });
            }
        }

        /// <summary>
        /// Отсоединить файл от продукта
        /// </summary>
        [HttpDelete("detach-from-product/{productId}/file/{fileId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> DetachFileFromProduct(Guid productId, Guid fileId)
        {
            try
            {
                var result = await _fileService.DetachFileFromProductAsync(productId, fileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detaching file {FileId} from product {ProductId}", fileId, productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при отсоединении файла от продукта"
                });
            }
        }

        /// <summary>
        /// Получить файлы продукта
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetProductFiles(Guid productId)
        {
            try
            {
                var result = await _fileService.GetFilesByProductIdAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов продукта"
                });
            }
        }

        /// <summary>
        /// Получить основные изображения продукта
        /// </summary>
        [HttpGet("product/{productId}/main-images")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetProductMainImages(Guid productId)
        {
            try
            {
                var result = await _fileService.GetMainImagesByProductIdAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main images for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении основных изображений"
                });
            }
        }

        /// <summary>
        /// Получить дополнительные изображения продукта
        /// </summary>
        [HttpGet("product/{productId}/additional-images")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetProductAdditionalImages(Guid productId)
        {
            try
            {
                var result = await _fileService.GetAdditionalImagesByProductIdAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional images for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении дополнительных изображений"
                });
            }
        }

        /// <summary>
        /// Получить файл по ID
        /// </summary>
        [HttpGet("{fileId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<FileDto>>> GetFile(Guid fileId)
        {
            try
            {
                var result = await _fileService.GetFileByIdAsync(fileId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileId}", fileId);
                return StatusCode(500, new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Ошибка при получении файла"
                });
            }
        }

        /// <summary>
        /// Получить файлы автора
        /// </summary>
        [HttpGet("author/{authorId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetFilesByAuthor(Guid authorId)
        {
            try
            {
                var result = await _fileService.GetFilesByAuthorAsync(authorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files by author {AuthorId}", authorId);
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов автора"
                });
            }
        }

        /// <summary>
        /// Получить мои файлы
        /// </summary>
        [HttpGet("my-files")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetMyFiles()
        {
            try
            {
                var authorId = GetCurrentUserId();
                var result = await _fileService.GetFilesByAuthorAsync(authorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's files");
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении ваших файлов"
                });
            }
        }

        /// <summary>
        /// Удалить файл по ID
        /// </summary>
        [HttpDelete("{fileId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(Guid fileId)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(fileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файла"
                });
            }
        }

        /// <summary>
        /// Удалить файл по PublicId (из Cloudinary)
        /// </summary>
        [HttpDelete("public/{publicId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFileByPublicId(string publicId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "PublicId обязателен"
                    });
                }

                var result = await _fileService.DeleteFileByPublicIdAsync(publicId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file by publicId {PublicId}", publicId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файла"
                });
            }
        }

        /// <summary>
        /// Удалить все файлы продукта
        /// </summary>
        [HttpDelete("product/{productId}/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProductFiles(Guid productId)
        {
            try
            {
                var result = await _fileService.DeleteProductFilesAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting files for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файлов продукта"
                });
            }
        }

        /// <summary>
        /// Получить статистику файлов
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<FileStatisticsDto>>> GetFileStatistics()
        {
            try
            {
                var result = await _fileService.GetFileStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file statistics");
                return StatusCode(500, new ApiResponse<FileStatisticsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики файлов"
                });
            }
        }

        /// <summary>
        /// Получить количество файлов пользователя
        /// </summary>
        [HttpGet("count/my")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<int>>> GetMyFilesCount()
        {
            try
            {
                var authorId = GetCurrentUserId();
                var result = await _fileService.GetFilesCountByAuthorAsync(authorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's files count");
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Ошибка при подсчете ваших файлов"
                });
            }
        }

        /// <summary>
        /// Получить неиспользуемые файлы
        /// </summary>
        [HttpGet("orphans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetOrphanFiles()
        {
            try
            {
                var result = await _fileService.GetOrphanFilesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orphan files");
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении неиспользуемых файлов"
                });
            }
        }

        /// <summary>
        /// Очистить неиспользуемые файлы
        /// </summary>
        [HttpDelete("cleanup-orphans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> CleanupOrphanFiles()
        {
            try
            {
                var result = await _fileService.CleanupOrphanFilesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphan files");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при очистке неиспользуемых файлов"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            }
            return userId;
        }
    }
}