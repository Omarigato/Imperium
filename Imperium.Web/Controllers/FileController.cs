using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Cloudinary;
using Imperium.Service.Services.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public FileController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            if (files.Count > 10)
                return BadRequest("Maximum 10 files allowed.");

            var results = new List<ApiResponse<CloudinaryResponse>>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    results.Add(new ApiResponse<CloudinaryResponse>
                    {
                        Success = false,
                        Message = $"File '{file.FileName}' is empty."
                    });
                    continue;
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    var result = await _cloudinaryService.UploadImage(stream);
                    result.Data.FileName = file.FileName;
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new ApiResponse<CloudinaryResponse>
                    {
                        Success = false,
                        Message = $"Error uploading file '{file.FileName}': {ex.Message}"
                    });
                }
            }

            return Ok(results);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest("publicId is required.");

            try
            {
                await _cloudinaryService.DeleteImage(publicId);
                return Ok($"Image with publicId '{publicId}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
