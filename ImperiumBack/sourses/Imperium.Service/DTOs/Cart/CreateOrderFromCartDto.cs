using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Cart
{
    /// <summary>
    /// DTO для создания заказа из корзины
    /// </summary>
    public class CreateOrderFromCartDto
    {
        public Guid? DeliveryAddressId { get; set; }
        public string? Notes { get; set; }

        // Выбранные товары из корзины (если не все)
        public List<Guid>? SelectedCartItemIds { get; set; }
    }
}
