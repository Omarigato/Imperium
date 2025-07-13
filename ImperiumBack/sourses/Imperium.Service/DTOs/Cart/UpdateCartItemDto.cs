using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Cart
{
    /// <summary>
    /// DTO для обновления количества в корзине
    /// </summary>
    public class UpdateCartItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }

        public Guid? SelectedColorId { get; set; }
        public Guid? SelectedSizeId { get; set; }
        public string? Notes { get; set; }
    }
}
