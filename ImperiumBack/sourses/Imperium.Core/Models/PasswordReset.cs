using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class PasswordReset
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
    }
}
