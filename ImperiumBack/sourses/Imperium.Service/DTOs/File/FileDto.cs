using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    // <summary>
    /// DTO файла
    /// </summary>
    public class FileDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int Size { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsAttachedToProduct { get; set; }
        public List<ProductFileDto> ProductAttachments { get; set; } = new();
    }
}
