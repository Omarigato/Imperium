using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        public bool IsPhoneVerified { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
