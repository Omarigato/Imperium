using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Auth
{
    public class EmailRegisterDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
