using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    /// <summary>
    /// DTO результата загрузки файла
    /// </summary>
    public class FileUploadResultDto
    {
        public FileDto File { get; set; } = null!;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }
}
