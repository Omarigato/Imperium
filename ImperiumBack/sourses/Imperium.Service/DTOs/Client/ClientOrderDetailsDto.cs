using Imperium.Service.DTOs.Address;
using Imperium.Service.DTOs.Order;
using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO деталей заказа клиента
    /// </summary>
    public class ClientOrderDetailsDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDate { get; set; }

        public AddressDto? DeliveryAddress { get; set; }
        public List<OrderItemDetailDto> Items { get; set; } = new();
    }
}
