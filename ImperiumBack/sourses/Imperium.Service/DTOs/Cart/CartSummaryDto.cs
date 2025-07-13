using System.Collections.Generic;

namespace Imperium.Service.DTOs.Cart
{
    /// <summary>
    /// DTO итогов корзины
    /// </summary>
    public class CartSummaryDto
    {
        public int ItemsCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        public List<CartDto> Items { get; set; } = new();
    }
}
