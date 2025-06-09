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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<ProductFile> ProductFiles { get; set; } = new List<ProductFile>();
    }
}