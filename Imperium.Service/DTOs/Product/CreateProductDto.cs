using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        public Guid CategoryId { get; set; }
        
        public Guid? MaterialId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string NameRu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string NameKz { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        
        public List<Guid> ColorIds { get; set; } = new();
        public List<Guid> SizeIds { get; set; } = new();
    }
}