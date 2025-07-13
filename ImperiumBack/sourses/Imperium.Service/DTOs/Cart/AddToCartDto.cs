using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Cart
{
    /// <summary>
    /// DTO для добавления товара в корзину
    /// </summary>
    public class AddToCartDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; } = 1;

        public Guid? SelectedColorId { get; set; }
        public Guid? SelectedSizeId { get; set; }
        public string? Notes { get; set; }
    }
}