using Imperium.Core.Enums;
using Imperium.Service.DTOs.Address;
using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO детальной информации о заказе
    /// </summary>
    public class OrderDetailsDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public AddressDto? DeliveryAddress { get; set; }

        public List<OrderItemDetailsDto> Items { get; set; } = new();

        public decimal FinalAmount => TotalAmount + DeliveryFee;
        public bool CanEdit => Status != OrderStatus.Confirmed && Status != OrderStatus.Cancelled;
    }
}
