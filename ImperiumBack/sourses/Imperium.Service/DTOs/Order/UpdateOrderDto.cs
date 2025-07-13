using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO для обновления заказа
    /// </summary>
    public class UpdateOrderDto
    {
        public Guid? DeliveryAddressId { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }

        public List<UpdateOrderItemDto>? Items { get; set; }
    }
}
