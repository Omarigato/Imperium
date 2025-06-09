using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Imperium.Core.Enums;

namespace Imperium.Core.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid? DeliveryAddressId { get; set; }
        public virtual Address? DeliveryAddress { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal DeliveryFee { get; set; } = 0;
        
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}