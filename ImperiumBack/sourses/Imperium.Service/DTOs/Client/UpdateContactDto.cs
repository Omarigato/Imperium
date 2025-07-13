using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO для обновления контактных данных
    /// </summary>
    public class UpdateContactDto
    {
        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;
    }
}
