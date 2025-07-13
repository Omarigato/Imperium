using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO элемента быстрого заказа
    /// </summary>
    public class QuickOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }

        public Guid? SelectedColorId { get; set; }
        public Guid? SelectedSizeId { get; set; }
        public string? Notes { get; set; }
    }
}
