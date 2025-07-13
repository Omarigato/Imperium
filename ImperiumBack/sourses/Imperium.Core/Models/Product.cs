using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imperium.Core.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid CategoryId { get; set; }
        public virtual Dictionary Category { get; set; } = null!;
        
        public Guid? MaterialId { get; set; }
        public virtual Dictionary? Material { get; set; }
        
        [Required]
        [StringLength(255)]
        public string NameRu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string NameKz { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }

        // Navigation properties
        public virtual ICollection<ProductFile> ProductFiles { get; set; } = new List<ProductFile>();
        public virtual ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
        public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}