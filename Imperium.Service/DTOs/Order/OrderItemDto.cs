using Imperium.Service.DTOs.Product;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.DTOs.Order
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ItemNotes { get; set; }
        
        public ProductDto Product { get; set; } = null!;
        public DictionaryDto? SelectedColor { get; set; }
        public DictionaryDto? SelectedSize { get; set; }
    }
}