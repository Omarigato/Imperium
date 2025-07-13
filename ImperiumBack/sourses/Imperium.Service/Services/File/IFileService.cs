using Imperium.Service.DTOs;
using Imperium.Service.DTOs.File;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.File
{
    public interface IFileService
    {
        // Загрузка файлов
        Task<ApiResponse<List<FileDto>>> UploadFilesAsync(List<IFormFile> files, Guid authorId, string folder = "imperium");
        Task<ApiResponse<FileDto>> UploadFileAsync(IFormFile file, Guid authorId, string folder = "imperium");

        // Связывание с продуктами
        Task<ApiResponse<bool>> AttachFilesToProductAsync(Guid productId, List<Guid> fileIds, Guid authorId, bool isAddition = false);
        Task<ApiResponse<bool>> DetachFileFromProductAsync(Guid productId, Guid fileId);

        // Получение файлов
        Task<ApiResponse<IEnumerable<FileDto>>> GetFilesByProductIdAsync(Guid productId);
        Task<ApiResponse<IEnumerable<FileDto>>> GetMainImagesByProductIdAsync(Guid productId);
        Task<ApiResponse<IEnumerable<FileDto>>> GetAdditionalImagesByProductIdAsync(Guid productId);
        Task<ApiResponse<FileDto>> GetFileByIdAsync(Guid fileId);
        Task<ApiResponse<IEnumerable<FileDto>>> GetFilesByAuthorAsync(Guid authorId);

        // Удаление файлов
        Task<ApiResponse<bool>> DeleteFileAsync(Guid fileId);
        Task<ApiResponse<bool>> DeleteFileByPublicIdAsync(string publicId);
        Task<ApiResponse<bool>> DeleteProductFilesAsync(Guid productId);

        // Статистика
        Task<ApiResponse<FileStatisticsDto>> GetFileStatisticsAsync();
        Task<ApiResponse<int>> GetFilesCountByAuthorAsync(Guid authorId);

        // Утилиты
        Task<ApiResponse<bool>> CleanupOrphanFilesAsync();
        Task<ApiResponse<IEnumerable<FileDto>>> GetOrphanFilesAsync();
    }
}
