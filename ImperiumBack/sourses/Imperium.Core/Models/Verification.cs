using Imperium.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Verification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        [Required]
        public VerificationType Type { get; set; } = VerificationType.Phone;

        [Required]
        [StringLength(255)]
        public string Contact { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpireDate { get; set; }
        public DateTime UsedDate { get; set; }
        public int AttemptCount { get; set; } = 0;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}