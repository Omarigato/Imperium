using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Dictionary
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string NameRu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string NameKz { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Value { get; set; }
        
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        
        public Guid? ParentId { get; set; }
        public virtual Dictionary? Parent { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Dictionary> Children { get; set; } = new List<Dictionary>();
        public virtual ICollection<Product> ProductsAsCategory { get; set; } = new List<Product>();
        public virtual ICollection<Product> ProductsAsMaterial { get; set; } = new List<Product>();
        public virtual ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
        public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        public virtual ICollection<Cart> CartsAsColor { get; set; } = new List<Cart>();
        public virtual ICollection<Cart> CartsAsSize { get; set; } = new List<Cart>();
        public virtual ICollection<OrderItem> OrderItemsAsColor { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderItem> OrderItemsAsSize { get; set; } = new List<OrderItem>();
    }
}