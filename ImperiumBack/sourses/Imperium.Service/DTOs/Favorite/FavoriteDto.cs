using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Favorite
{
    // <summary>
    /// DTO элемента избранного
    /// </summary>
    public class FavoriteDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreateDate { get; set; }

        // Информация о продукте
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public List<string> ProductImages { get; set; } = new();
        public string CategoryName { get; set; } = string.Empty;
    }
}
