using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    /// <summary>
    /// DTO множественной загрузки файлов
    /// </summary>
    public class MultipleFileUploadResultDto
    {
        public List<FileUploadResultDto> Results { get; set; } = new();
        public int SuccessCount => Results.Count(r => r.Success);
        public int FailedCount => Results.Count(r => !r.Success);
        public bool AllSuccessful => Results.All(r => r.Success);
        public string Summary => $"Успешно: {SuccessCount}, Ошибок: {FailedCount}";
    }
}
