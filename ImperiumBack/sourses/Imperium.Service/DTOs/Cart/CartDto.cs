using System;
using Imperium.Service.DTOs.Product;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.DTOs.Cart
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public ProductDto Product { get; set; } = null!;
        public DictionaryDto? SelectedColor { get; set; }
        public DictionaryDto? SelectedSize { get; set; }
        
        public decimal TotalPrice => Product.Price * Quantity;
    }
}