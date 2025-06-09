using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Cloudinary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Cloudinary
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var cloudName = _configuration["Cloudinary:CloudName"] ?? throw new ArgumentNullException("Cloudinary:CloudName");
            var apiKey = _configuration["Cloudinary:ApiKey"] ?? throw new ArgumentNullException("Cloudinary:ApiKey");
            var apiSecret = _configuration["Cloudinary:ApiSecret"] ?? throw new ArgumentNullException("Cloudinary:ApiSecret");

            _cloudinary = new CloudinaryDotNet.Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        public async Task<ApiResponse<CloudinaryResponse>> UploadImage(Stream file, string folderName = "imperium")
        {
            try
            {
                // Проверка mime-типа (на основе сигнатуры файла)
                if (!IsImage(file))
                {
                    return new ApiResponse<CloudinaryResponse>
                    {
                        Success = false,
                        Message = "Only image files are allowed."
                    };
                }

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription("image", file),
                    Folder = folderName,
                    Transformation = new Transformation().Width(800).Height(800).Crop("limit")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    return new ApiResponse<CloudinaryResponse>
                    {
                        Success = false,
                        Message = result.Error.Message
                    };
                }

                return new ApiResponse<CloudinaryResponse>
                {
                    Data = new CloudinaryResponse
                    {
                        PublicId = result.PublicId,
                        Url = result.SecureUrl.ToString()
                    },
                    Success = result.StatusCode == System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in UploadImage with folder {FolderName}", folderName);
                return new ApiResponse<CloudinaryResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        public async Task<ApiResponse<object>> DeleteImage(string publicId)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                if (result.Error != null)
                {
                    throw new Exception(result.Error.Message);
                }

                _logger.LogDebug("File successfully deleted: {PublicId}, Status: {Result}", publicId, result);

                return new ApiResponse<object>
                {
                    Success = result.StatusCode == System.Net.HttpStatusCode.OK,
                    Message = "File successfully deleted."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in DeleteImage with publicId {PublicId}", publicId);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private bool IsImage(Stream stream)
        {
            try
            {
                // Читаем сигнатуру (максимум 8 байт)
                byte[] header = new byte[8];
                stream.Position = 0;
                stream.Read(header, 0, header.Length);
                stream.Position = 0;

                // Проверяем сигнатуру популярных форматов
                // JPEG
                if (header[0] == 0xFF && header[1] == 0xD8) return true;
                // PNG
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E &&
                    header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A &&
                    header[6] == 0x1A && header[7] == 0x0A) return true;
                // GIF
                if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}
