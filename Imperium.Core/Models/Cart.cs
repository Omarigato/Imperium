using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
        
        public Guid? SelectedColorId { get; set; }
        public virtual Core.Models.Dictionary? SelectedColor { get; set; }
        
        public Guid? SelectedSizeId { get; set; }
        public virtual Dictionary? SelectedSize { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}