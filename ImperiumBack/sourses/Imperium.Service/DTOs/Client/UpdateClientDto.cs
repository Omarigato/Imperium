using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO для обновления информации клиента
    /// </summary>
    public class UpdateClientDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
}
