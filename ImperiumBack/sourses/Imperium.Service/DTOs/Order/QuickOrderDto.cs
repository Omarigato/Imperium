using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO для быстрого создания заказа
    /// </summary>
    public class QuickOrderDto
    {
        public Guid? DeliveryAddressId { get; set; }
        public string? Notes { get; set; }

        [Required]
        public List<QuickOrderItemDto> Items { get; set; } = new();
    }
}
