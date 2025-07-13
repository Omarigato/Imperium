using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Admin
{
    // <summary>
    /// DTO для создания нового администратора
    /// </summary>
    public class CreateAdminDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false; // По умолчанию создаем менеджера
    }
}
