using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Cart
{
    public class AddToCartDto
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
        
        public Guid? SelectedColorId { get; set; }
        public Guid? SelectedSizeId { get; set; }
        public string? Notes { get; set; }
    }
}