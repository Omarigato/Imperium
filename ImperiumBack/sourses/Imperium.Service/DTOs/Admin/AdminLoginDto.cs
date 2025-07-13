using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Admin
{
    /// <summary>
    /// DTO для входа администратора
    /// </summary>
    public class AdminLoginDto
    {
        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
