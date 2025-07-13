using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    /// <summary>
    /// DTO связи файла с продуктом
    /// </summary>
    public class ProductFileDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public bool IsAddition { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
