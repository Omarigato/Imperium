using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    /// <summary>
    /// DTO для прикрепления файлов к продукту
    /// </summary>
    public class AttachFilesToProductDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public List<Guid> FileIds { get; set; } = new();

        public bool IsAddition { get; set; } = false;
    }
}
