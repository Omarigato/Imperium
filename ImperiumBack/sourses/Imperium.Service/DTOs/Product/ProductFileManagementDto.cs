using Imperium.Service.DTOs.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Product
{
    /// <summary>
    /// DTO для управления файлами продукта
    /// </summary>
    public class ProductFileManagementDto
    {
        public Guid ProductId { get; set; }
        public List<FileDto> AllFiles { get; set; } = new();
        public List<FileDto> MainImages { get; set; } = new();
        public List<FileDto> AdditionalImages { get; set; } = new();
        public int TotalFilesCount => AllFiles.Count;
        public int MainImagesCount => MainImages.Count;
        public int AdditionalImagesCount => AdditionalImages.Count;
    }
}
