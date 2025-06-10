using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Cloudinary;
using System.IO;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Cloudinary
{
    public interface ICloudinaryService
    {
        Task<ApiResponse<CloudinaryResponse>> UploadImage(Stream file, string folderName = "imperium");
        Task<ApiResponse<object>> DeleteImage(string publicId);
    }
}