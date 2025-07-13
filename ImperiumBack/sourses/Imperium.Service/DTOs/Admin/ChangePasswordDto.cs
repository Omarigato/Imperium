using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Admin
{
    /// <summary>
    /// DTO для смены пароля
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
