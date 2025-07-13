using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO элемента заказа с деталями
    /// </summary>
    public class OrderItemDetailDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? ItemNotes { get; set; }

        public List<string> ProductImages { get; set; } = new();
    }
}
