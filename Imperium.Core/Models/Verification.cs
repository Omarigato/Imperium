using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Verification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = string.Empty; // Email или Phone

        [Required]
        [StringLength(255)]
        public string Contact { get; set; } = string.Empty; // email или phone

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsVerified { get; set; } = false;

        public int AttemptCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}