using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Address
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        [StringLength(100)]
        public string? Title { get; set; }
        
        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string HouseNumber { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Apartment { get; set; }
        
        public string? Notes { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}