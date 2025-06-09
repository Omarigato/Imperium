using System;
using System.Collections.Generic;
using Imperium.Service.DTOs.Dictionary;

namespace Imperium.Service.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string NameRu { get; set; } = string.Empty;
        public string NameKz { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public DictionaryDto Category { get; set; } = null!;
        public DictionaryDto? Material { get; set; }
        public List<DictionaryDto> AvailableColors { get; set; } = new();
        public List<DictionaryDto> AvailableSizes { get; set; } = new();
        public List<string> Images { get; set; } = new();
    }
}