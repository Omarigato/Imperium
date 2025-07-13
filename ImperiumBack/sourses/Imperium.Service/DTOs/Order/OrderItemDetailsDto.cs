using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO элемента заказа с деталями
    /// </summary>
    public class OrderItemDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public string? SelectedColorName { get; set; }
        public string? SelectedSizeName { get; set; }
        public string? ItemNotes { get; set; }

        public List<string> ProductImages { get; set; } = new();
    }
}
