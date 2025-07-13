using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Admin
{
    /// <summary>
    /// DTO результата массового удаления
    /// </summary>
    public class BulkDeleteResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessfulDeletes { get; set; }
        public int FailedDeletes { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool AllSuccessful => FailedDeletes == 0;
        public string Summary => $"Успешно: {SuccessfulDeletes}, Ошибок: {FailedDeletes}";
    }
}
