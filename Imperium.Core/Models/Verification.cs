using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Verification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        
        public DateTime ExpireDate { get; set; }
        public DateTime? TotalExpireDate { get; set; }
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}