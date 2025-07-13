using Imperium.Core.Enums;
using System;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO краткой информации о заказе
    /// </summary>
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public DateTime CreateDate { get; set; }
        public int ItemsCount { get; set; }

        // Информация о клиенте (для админов)
        public string? ClientName { get; set; }
        public string? ClientPhone { get; set; }
        public string? DeliveryAddress { get; set; }

        public decimal FinalAmount => TotalAmount + DeliveryFee;
    }
}
