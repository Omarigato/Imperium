using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Order
{
    public class CreateOrderDto
    {
        public Guid? DeliveryAddressId { get; set; }
        public string? Notes { get; set; }
    }
}