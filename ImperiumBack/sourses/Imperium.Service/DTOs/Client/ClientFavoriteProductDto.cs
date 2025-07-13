using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO часто заказываемых товаров клиента
    /// </summary>
    public class ClientFavoriteProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int OrderCount { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime LastOrderDate { get; set; }

        public List<string> ProductImages { get; set; } = new();
    }
}
