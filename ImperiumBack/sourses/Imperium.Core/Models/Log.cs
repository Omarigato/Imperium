using System;
using System.ComponentModel.DataAnnotations;
namespace Imperium.Core.Models
{
    public class Log
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(20)]
        public string Level { get; set; } = string.Empty; // Info, Warning, Error, Debug

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? Exception { get; set; }

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        [StringLength(500)]
        public string? RequestPath { get; set; }

        [StringLength(10)]
        public string? RequestMethod { get; set; }

        [StringLength(45)]
        public string? IPAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
