using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Cart
{
    /// <summary>
    /// DTO элемента корзины
    /// </summary>
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDate { get; set; }

        // Информация о продукте
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        // Выбранные опции
        public string? SelectedColorName { get; set; }
        public string? SelectedSizeName { get; set; }
        public Guid? SelectedColorId { get; set; }
        public Guid? SelectedSizeId { get; set; }

        public List<string> ProductImages { get; set; } = new();
        public string CategoryName { get; set; } = string.Empty;

        // Вычисляемые поля
        public decimal TotalPrice => Price * Quantity;
    }

}