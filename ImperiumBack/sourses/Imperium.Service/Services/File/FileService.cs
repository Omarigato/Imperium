using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Product;
using Imperium.Data.Repositories.ProductFile;
using Imperium.Data.Repositories.User;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.File;
using Imperium.Service.Services.Cloudinary;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.File
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IProductFileRepository _productFileRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IFileRepository fileRepository,
            IProductFileRepository productFileRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            ICloudinaryService cloudinaryService,
            IMapper mapper,
            ILogger<FileService> logger)
        {
            _fileRepository = fileRepository;
            _productFileRepository = productFileRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<List<FileDto>>> UploadFilesAsync(List<IFormFile> files, Guid authorId, string folder = "imperium")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return new ApiResponse<List<FileDto>>
                    {
                        Success = false,
                        Message = "Файлы не выбраны для загрузки"
                    };
                }

                if (files.Count > 10)
                {
                    return new ApiResponse<List<FileDto>>
                    {
                        Success = false,
                        Message = "Максимальное количество файлов для загрузки: 10"
                    };
                }

                var author = await _userRepository.GetByIdAsync(authorId);
                if (author == null)
                {
                    return new ApiResponse<List<FileDto>>
                    {
                        Success = false,
                        Message = "Пользователь не найден"
                    };
                }

                var uploadResults = new List<FileDto>();
                var errors = new List<string>();

                foreach (var file in files)
                {
                    try
                    {
                        var result = await UploadSingleFileAsync(file, authorId, folder);
                        if (result.Success && result.Data != null)
                        {
                            uploadResults.Add(result.Data);
                        }
                        else
                        {
                            errors.Add($"{file.FileName}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                        errors.Add($"{file.FileName}: Ошибка загрузки");
                    }
                }

                var message = errors.Any()
                    ? $"Загружено {uploadResults.Count} из {files.Count} файлов. Ошибки: {string.Join(", ", errors)}"
                    : $"Успешно загружено {uploadResults.Count} файлов";

                return new ApiResponse<List<FileDto>>
                {
                    Success = uploadResults.Any(),
                    Data = uploadResults,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadFilesAsync");
                return new ApiResponse<List<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при загрузке файлов"
                };
            }
        }

        public async Task<ApiResponse<FileDto>> UploadFileAsync(IFormFile file, Guid authorId, string folder = "imperium")
        {
            return await UploadSingleFileAsync(file, authorId, folder);
        }

        private async Task<ApiResponse<FileDto>> UploadSingleFileAsync(IFormFile file, Guid authorId, string folder)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Файл пустой"
                };
            }

            // Проверяем размер файла (максимум 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Размер файла не должен превышать 10MB"
                };
            }

            try
            {
                // Загружаем в Cloudinary
                using var stream = file.OpenReadStream();
                var cloudinaryResult = await _cloudinaryService.UploadImage(stream, folder);

                if (!cloudinaryResult.Success || cloudinaryResult.Data == null)
                {
                    return new ApiResponse<FileDto>
                    {
                        Success = false,
                        Message = $"Ошибка загрузки в облако: {cloudinaryResult.Message}"
                    };
                }

                // Сохраняем в БД
                var fileEntity = new Core.Models.File
                {
                    Url = cloudinaryResult.Data.Url!,
                    PublicId = cloudinaryResult.Data.PublicId!,
                    FileName = file.FileName,
                    Size = (int)file.Length,
                    MimeType = file.ContentType,
                    AuthorId = authorId,
                    CreateDate = DateTime.UtcNow
                };

                await _fileRepository.Insert(fileEntity);

                var author = await _userRepository.GetByIdAsync(authorId);
                var fileDto = _mapper.Map<FileDto>(fileEntity);
                fileDto.AuthorName = author?.FullName;

                _logger.LogInformation("File uploaded successfully: {FileName} by user {AuthorId}", file.FileName, authorId);

                return new ApiResponse<FileDto>
                {
                    Success = true,
                    Data = fileDto,
                    Message = "Файл успешно загружен"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                return new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Ошибка при загрузке файла"
                };
            }
        }

        public async Task<ApiResponse<bool>> AttachFilesToProductAsync(Guid productId, List<Guid> fileIds, Guid authorId, bool isAddition = false)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Продукт не найден"
                    };
                }

                var attachedCount = 0;
                var errors = new List<string>();

                foreach (var fileId in fileIds)
                {
                    var file = await _fileRepository.GetByIdAsync(fileId);
                    if (file == null)
                    {
                        errors.Add($"Файл {fileId} не найден");
                        continue;
                    }

                    // Проверяем, не прикреплен ли уже
                    var existing = await _productFileRepository.ExistsAsync(productId, fileId);
                    if (existing)
                    {
                        errors.Add($"Файл {file.FileName} уже прикреплен к продукту");
                        continue;
                    }

                    var productFile = new ProductFile
                    {
                        ProductId = productId,
                        FileId = fileId,
                        IsAddition = isAddition,
                        AuthorId = authorId,
                        CreateDate = DateTime.UtcNow
                    };

                    await _productFileRepository.Insert(productFile);
                    attachedCount++;
                }

                var message = errors.Any()
                    ? $"Прикреплено {attachedCount} файлов. Ошибки: {string.Join(", ", errors)}"
                    : $"Успешно прикреплено {attachedCount} файлов к продукту";

                _logger.LogInformation("Files attached to product {ProductId}: {AttachedCount} successful, {ErrorCount} errors",
                    productId, attachedCount, errors.Count);

                return new ApiResponse<bool>
                {
                    Success = attachedCount > 0,
                    Data = attachedCount > 0,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching files to product {ProductId}", productId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при прикреплении файлов к продукту"
                };
            }
        }

        public async Task<ApiResponse<bool>> DetachFileFromProductAsync(Guid productId, Guid fileId)
        {
            try
            {
                var success = await _productFileRepository.DeleteByProductAndFileAsync(productId, fileId);

                if (success)
                {
                    _logger.LogInformation("File {FileId} detached from product {ProductId}", fileId, productId);
                }

                return new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Файл отсоединен от продукта" : "Связь файла с продуктом не найдена"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detaching file {FileId} from product {ProductId}", fileId, productId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при отсоединении файла от продукта"
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<FileDto>>> GetFilesByProductIdAsync(Guid productId)
        {
            try
            {
                var files = await _fileRepository.GetByProductIdAsync(productId);
                var fileDtos = new List<FileDto>();

                foreach (var file in files)
                {
                    var author = await _userRepository.GetByIdAsync(file.AuthorId);
                    var fileDto = _mapper.Map<FileDto>(file);
                    fileDto.AuthorName = author?.FullName;
                    fileDto.IsAttachedToProduct = true;
                    fileDtos.Add(fileDto);
                }

                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = true,
                    Data = fileDtos,
                    Message = $"Найдено {fileDtos.Count} файлов для продукта"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files for product {ProductId}", productId);
                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов продукта"
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<FileDto>>> GetMainImagesByProductIdAsync(Guid productId)
        {
            try
            {
                var productFiles = await _productFileRepository.GetMainImagesByProductIdAsync(productId);
                var fileDtos = new List<FileDto>();

                foreach (var productFile in productFiles)
                {
                    var file = await _fileRepository.GetByIdAsync(productFile.FileId);
                    if (file != null)
                    {
                        var author = await _userRepository.GetByIdAsync(file.AuthorId);
                        var fileDto = _mapper.Map<FileDto>(file);
                        fileDto.AuthorName = author?.FullName;
                        fileDto.IsAttachedToProduct = true;
                        fileDtos.Add(fileDto);
                    }
                }

                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = true,
                    Data = fileDtos,
                    Message = $"Найдено {fileDtos.Count} основных изображений"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main images for product {ProductId}", productId);
                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении основных изображений"
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<FileDto>>> GetAdditionalImagesByProductIdAsync(Guid productId)
        {
            try
            {
                var productFiles = await _productFileRepository.GetAdditionalImagesByProductIdAsync(productId);
                var fileDtos = new List<FileDto>();

                foreach (var productFile in productFiles)
                {
                    var file = await _fileRepository.GetByIdAsync(productFile.FileId);
                    if (file != null)
                    {
                        var author = await _userRepository.GetByIdAsync(file.AuthorId);
                        var fileDto = _mapper.Map<FileDto>(file);
                        fileDto.AuthorName = author?.FullName;
                        fileDto.IsAttachedToProduct = true;
                        fileDtos.Add(fileDto);
                    }
                }

                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = true,
                    Data = fileDtos,
                    Message = $"Найдено {fileDtos.Count} дополнительных изображений"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional images for product {ProductId}", productId);
                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении дополнительных изображений"
                };
            }
        }

        public async Task<ApiResponse<FileDto>> GetFileByIdAsync(Guid fileId)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(fileId);
                if (file == null)
                {
                    return new ApiResponse<FileDto>
                    {
                        Success = false,
                        Message = "Файл не найден"
                    };
                }

                var author = await _userRepository.GetByIdAsync(file.AuthorId);
                var fileDto = _mapper.Map<FileDto>(file);
                fileDto.AuthorName = author?.FullName;

                // Получаем связи с продуктами
                var productFiles = await _productFileRepository.GetByFileIdAsync(fileId);
                foreach (var pf in productFiles)
                {
                    var product = await _productRepository.GetByIdAsync(pf.ProductId);
                    fileDto.ProductAttachments.Add(new ProductFileDto
                    {
                        Id = pf.Id,
                        ProductId = pf.ProductId,
                        ProductName = product?.NameRu,
                        IsAddition = pf.IsAddition,
                        CreateDate = pf.CreateDate
                    });
                }

                fileDto.IsAttachedToProduct = fileDto.ProductAttachments.Any();

                return new ApiResponse<FileDto>
                {
                    Success = true,
                    Data = fileDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileId}", fileId);
                return new ApiResponse<FileDto>
                {
                    Success = false,
                    Message = "Ошибка при получении файла"
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<FileDto>>> GetFilesByAuthorAsync(Guid authorId)
        {
            try
            {
                var files = await _fileRepository.GetByAuthorAsync(authorId);
                var author = await _userRepository.GetByIdAsync(authorId);

                var fileDtos = files.Select(f =>
                {
                    var dto = _mapper.Map<FileDto>(f);
                    dto.AuthorName = author?.FullName;
                    return dto;
                });

                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = true,
                    Data = fileDtos,
                    Message = $"Найдено {fileDtos.Count()} файлов автора"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files by author {AuthorId}", authorId);
                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении файлов автора"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileAsync(Guid fileId)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(fileId);
                if (file == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Файл не найден"
                    };
                }

                // Удаляем из Cloudinary
                var cloudinaryResult = await _cloudinaryService.DeleteImage(file.PublicId);
                if (!cloudinaryResult.Success)
                {
                    _logger.LogWarning("Failed to delete image from Cloudinary: {PublicId}", file.PublicId);
                }

                // Помечаем как удаленный в БД
                file.DeleteDate = DateTime.UtcNow;
                await _fileRepository.Update(file);

                _logger.LogInformation("File deleted: {FileId} ({FileName})", fileId, file.FileName);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Файл удален"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файла"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileByPublicIdAsync(string publicId)
        {
            try
            {
                var file = await _fileRepository.GetByPublicIdAsync(publicId);
                if (file == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Файл не найден"
                    };
                }

                return await DeleteFileAsync(file.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file by publicId {PublicId}", publicId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файла"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductFilesAsync(Guid productId)
        {
            try
            {
                var productFiles = await _productFileRepository.GetByProductIdAsync(productId);
                var deletedCount = 0;

                foreach (var productFile in productFiles)
                {
                    productFile.DeleteDate = DateTime.UtcNow;
                    await _productFileRepository.Update(productFile);
                    deletedCount++;
                }

                _logger.LogInformation("Deleted {Count} file associations for product {ProductId}", deletedCount, productId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Удалено {deletedCount} связей файлов с продуктом"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product files for product {ProductId}", productId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении файлов продукта"
                };
            }
        }

        public async Task<ApiResponse<FileStatisticsDto>> GetFileStatisticsAsync()
        {
            try
            {
                var allFiles = await _fileRepository.GetAllAsync();
                var activeFiles = allFiles.Where(f => f.DeleteDate == null).ToList();

                var orphanFiles = new List<Core.Models.File>();
                var attachedFiles = new List<Core.Models.File>();

                foreach (var file in activeFiles)
                {
                    var productFiles = await _productFileRepository.GetByFileIdAsync(file.Id);
                    if (productFiles.Any(pf => pf.DeleteDate == null))
                    {
                        attachedFiles.Add(file);
                    }
                    else
                    {
                        orphanFiles.Add(file);
                    }
                }

                var stats = new FileStatisticsDto
                {
                    TotalFiles = activeFiles.Count,
                    AttachedFiles = attachedFiles.Count,
                    OrphanFiles = orphanFiles.Count,
                    TotalSizeBytes = activeFiles.Sum(f => (long)f.Size),
                    OldestFileDate = activeFiles.Any() ? activeFiles.Min(f => f.CreateDate) : null,
                    NewestFileDate = activeFiles.Any() ? activeFiles.Max(f => f.CreateDate) : null,
                    FileTypeDistribution = activeFiles
                        .GroupBy(f => f.MimeType)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return new ApiResponse<FileStatisticsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Статистика файлов получена"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file statistics");
                return new ApiResponse<FileStatisticsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики файлов"
                };
            }
        }

        public async Task<ApiResponse<int>> GetFilesCountByAuthorAsync(Guid authorId)
        {
            try
            {
                var files = await _fileRepository.GetByAuthorAsync(authorId);
                var count = files.Count();

                return new ApiResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = $"Найдено {count} файлов автора"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files count for author {AuthorId}", authorId);
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "Ошибка при подсчете файлов автора"
                };
            }
        }

        public async Task<ApiResponse<bool>> CleanupOrphanFilesAsync()
        {
            try
            {
                var orphanFiles = await GetOrphanFilesInternalAsync();
                var deletedCount = 0;

                foreach (var file in orphanFiles)
                {
                    // Удаляем из Cloudinary
                    await _cloudinaryService.DeleteImage(file.PublicId);

                    // Помечаем как удаленный в БД
                    file.DeleteDate = DateTime.UtcNow;
                    await _fileRepository.Update(file);
                    deletedCount++;
                }

                _logger.LogInformation("Cleaned up {Count} orphan files", deletedCount);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Удалено {deletedCount} неиспользуемых файлов"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphan files");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при очистке неиспользуемых файлов"
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<FileDto>>> GetOrphanFilesAsync()
        {
            try
            {
                var orphanFiles = await GetOrphanFilesInternalAsync();
                var fileDtos = new List<FileDto>();

                foreach (var file in orphanFiles)
                {
                    var author = await _userRepository.GetByIdAsync(file.AuthorId);
                    var fileDto = _mapper.Map<FileDto>(file);
                    fileDto.AuthorName = author?.FullName;
                    fileDto.IsAttachedToProduct = false;
                    fileDtos.Add(fileDto);
                }

                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = true,
                    Data = fileDtos,
                    Message = $"Найдено {fileDtos.Count} неиспользуемых файлов"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orphan files");
                return new ApiResponse<IEnumerable<FileDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении неиспользуемых файлов"
                };
            }
        }

        private async Task<List<Core.Models.File>> GetOrphanFilesInternalAsync()
        {
            var allFiles = await _fileRepository.GetAllAsync();
            var activeFiles = allFiles.Where(f => f.DeleteDate == null);
            var orphanFiles = new List<Core.Models.File>();

            foreach (var file in activeFiles)
            {
                var productFiles = await _productFileRepository.GetByFileIdAsync(file.Id);
                if (!productFiles.Any(pf => pf.DeleteDate == null))
                {
                    orphanFiles.Add(file);
                }
            }

            return orphanFiles;
        }
    }
}