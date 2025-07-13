using System;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO истории заказов клиента
    /// </summary>
    public class ClientOrderHistoryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public DateTime CreateDate { get; set; }
        public int ItemsCount { get; set; }

        public string? DeliveryAddress { get; set; }
    }
}
