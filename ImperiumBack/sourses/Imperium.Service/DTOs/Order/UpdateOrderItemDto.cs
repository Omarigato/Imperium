using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO для обновления элемента заказа
    /// </summary>
    public class UpdateOrderItemDto
    {
        [Required]
        public Guid Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
