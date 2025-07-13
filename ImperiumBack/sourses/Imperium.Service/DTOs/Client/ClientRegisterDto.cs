using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO для регистрации клиента
    /// </summary>
    public class ClientRegisterDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
}
