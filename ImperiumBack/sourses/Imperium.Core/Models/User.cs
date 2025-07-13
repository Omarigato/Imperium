using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Imperium.Core.Enums;

namespace Imperium.Core.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Login { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        public UserRole Role { get; set; } = UserRole.Manager;

        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Verification> Verifications { get; set; } = new List<Verification>();
    }
}