using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class File
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string Url { get; set; } = string.Empty;
        
        [Required]
        public string PublicId { get; set; } = string.Empty;
        
        [Required]
        public string FileName { get; set; } = string.Empty;
        
        public int Size { get; set; }
        
        [Required]
        public string MimeType { get; set; } = string.Empty;

        [Required]
        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }

        // Navigation properties
        public virtual ICollection<ProductFile> ProductFiles { get; set; } = new List<ProductFile>();
    }
}