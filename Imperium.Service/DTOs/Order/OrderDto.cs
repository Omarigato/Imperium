using Imperium.Core.Enums;
using Imperium.Service.DTOs.User;

namespace Imperium.Service.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public UserDto User { get; set; } = null!;
        public AddressDto? DeliveryAddress { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}