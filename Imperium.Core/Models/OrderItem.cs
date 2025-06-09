using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imperium.Core.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        
        public Guid? SelectedColorId { get; set; }
        public virtual Dictionary? SelectedColor { get; set; }
        
        public Guid? SelectedSizeId { get; set; }
        public virtual Dictionary? SelectedSize { get; set; }
        
        public string? ItemNotes { get; set; }
    }
}