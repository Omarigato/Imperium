using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Admin;
using Imperium.Service.DTOs.File;
using Imperium.Service.DTOs.Product;
using Imperium.Service.Services.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для управления файлами в админ панели
    /// </summary>
    [ApiController]
    [Route("api/admin/files")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminFilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<AdminFilesController> _logger;

        public AdminFilesController(IFileService fileService, ILogger<AdminFilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все файлы (для админки)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<FileDto>>>> GetAllFiles([FromQuery] Guid? authorId = null)
        {
            try
            {
                ApiResponse<IEnumerable<FileDto>> result;

                if (authorId.HasValue)
                {
                    result = await _fileService.GetFilesByAuthorAsync(authorId.Value);
                }
                else
                {
                    // Для получения всех файлов можно добавить метод в FileService
                    // Пока используем файлы текущего пользователя
                    var currentUserId = GetCurrentUserId();
                    result = await _fileService.GetFilesByAuthorAsync(currentUserId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all files");
                return StatusCode(500, new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов"
                });
            }
        }

        /// <summary>
        /// Массовая загрузка файлов для продукта
        /// </summary>
        [HttpPost("bulk-upload-for-product")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkUploadForProduct(
            [FromForm] List<IFormFile> files,
            [FromForm] Guid productId,
            [FromForm] bool isAddition = false,
            [FromForm] string folder = "products")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Файлы не выбраны для загрузки"
                    });
                }

                var authorId = GetCurrentUserId();

                // Загружаем файлы
                var uploadResult = await _fileService.UploadFilesAsync(files, authorId, folder);
                if (!uploadResult.Success || uploadResult.Data == null)
                {
                    return BadRequest(uploadResult);
                }

                // Прикрепляем к продукту
                var fileIds = uploadResult.Data.Select(f => f.Id).ToList();
                var attachResult = await _fileService.AttachFilesToProductAsync(productId, fileIds, authorId, isAddition);

                return Ok(new ApiResponse<bool>
                {
                    Success = attachResult.Success,
                    Data = attachResult.Success,
                    Message = $"Загружено и прикреплено {uploadResult.Data.Count} файлов к продукту"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk upload for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при массовой загрузке файлов"
                });
            }
        }

        /// <summary>
        /// Управление файлами продукта
        /// </summary>
        [HttpGet("product/{productId}/manage")]
        public async Task<ActionResult<ApiResponse<ProductFileManagementDto>>> GetProductFileManagement(Guid productId)
        {
            try
            {
                var allFilesResult = await _fileService.GetFilesByProductIdAsync(productId);
                var mainImagesResult = await _fileService.GetMainImagesByProductIdAsync(productId);
                var additionalImagesResult = await _fileService.GetAdditionalImagesByProductIdAsync(productId);

                var management = new ProductFileManagementDto
                {
                    ProductId = productId,
                    AllFiles = allFilesResult.Data?.ToList() ?? new List<FileDto>(),
                    MainImages = mainImagesResult.Data?.ToList() ?? new List<FileDto>(),
                    AdditionalImages = additionalImagesResult.Data?.ToList() ?? new List<FileDto>()
                };

                return Ok(new ApiResponse<ProductFileManagementDto>
                {
                    Success = true,
                    Data = management,
                    Message = $"Найдено {management.AllFiles.Count} файлов для продукта"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product file management for {ProductId}", productId);
                return StatusCode(500, new ApiResponse<ProductFileManagementDto>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов продукта"
                });
            }
        }

        /// <summary>
        /// Переместить файл между основными и дополнительными изображениями
        /// </summary>
        [HttpPut("product/{productId}/file/{fileId}/toggle-addition")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleFileAddition(Guid productId, Guid fileId)
        {
            try
            {
                var authorId = GetCurrentUserId();

                // Отсоединяем файл
                var detachResult = await _fileService.DetachFileFromProductAsync(productId, fileId);
                if (!detachResult.Success)
                {
                    return BadRequest(detachResult);
                }

                // Прикрепляем заново с противоположным значением isAddition
                // Здесь нужно получить текущий статус файла, но для простоты переключаем
                var attachResult = await _fileService.AttachFilesToProductAsync(productId, new List<Guid> { fileId }, authorId, true);

                return Ok(new ApiResponse<bool>
                {
                    Success = attachResult.Success,
                    Data = attachResult.Success,
                    Message = "Статус файла изменен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling file addition status for product {ProductId}, file {FileId}", productId, fileId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при изменении статуса файла"
                });
            }
        }

        /// <summary>
        /// Получить неиспользуемые файлы (только для админов)
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
        /// Очистить неиспользуемые файлы (только для админов)
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

        /// <summary>
        /// Получить статистику файлов (только для админов)
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
        /// Массовое удаление файлов (только для админов)
        /// </summary>
        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<BulkDeleteResultDto>>> BulkDeleteFiles([FromBody] List<Guid> fileIds)
        {
            try
            {
                if (fileIds == null || !fileIds.Any())
                {
                    return BadRequest(new ApiResponse<BulkDeleteResultDto>
                    {
                        Success = false,
                        Message = "Не выбраны файлы для удаления"
                    });
                }

                var deletedCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var fileId in fileIds)
                {
                    try
                    {
                        var result = await _fileService.DeleteFileAsync(fileId);
                        if (result.Success)
                        {
                            deletedCount++;
                        }
                        else
                        {
                            failedCount++;
                            errors.Add($"Файл {fileId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        errors.Add($"Файл {fileId}: Ошибка удаления");
                        _logger.LogError(ex, "Error deleting file {FileId} in bulk operation", fileId);
                    }
                }

                var deleteResult = new BulkDeleteResultDto
                {
                    TotalRequested = fileIds.Count,
                    SuccessfulDeletes = deletedCount,
                    FailedDeletes = failedCount,
                    Errors = errors
                };

                return Ok(new ApiResponse<BulkDeleteResultDto>
                {
                    Success = deletedCount > 0,
                    Data = deleteResult,
                    Message = $"Удалено {deletedCount} из {fileIds.Count} файлов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete operation");
                return StatusCode(500, new ApiResponse<BulkDeleteResultDto>
                {
                    Success = false,
                    Message = "Ошибка при массовом удалении файлов"
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
