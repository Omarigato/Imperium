using System;

namespace Imperium.Service.DTOs.Dictionary
{
    public class DictionaryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string NameRu { get; set; } = string.Empty;
        public string NameKz { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? DescriptionRu { get; set; }
        public string? DescriptionKz { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
    }
}