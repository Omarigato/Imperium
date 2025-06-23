using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Auth
{
    public class PhoneRegisterDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
