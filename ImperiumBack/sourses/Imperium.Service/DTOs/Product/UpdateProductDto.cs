using System;

namespace Imperium.Service.DTOs.Product
{
    public class UpdateProductDto
    {
        public Guid CategoryId { get; set; }
        public Guid? MaterialId { get; set; }
        public string NameRu { get; set; } = string.Empty;
        public string NameKz { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
