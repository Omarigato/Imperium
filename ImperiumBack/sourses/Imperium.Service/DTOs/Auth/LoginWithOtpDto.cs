using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Auth
{
    public class LoginWithOtpDto
    {
        [Required]
        public string Contact { get; set; } = string.Empty; // Email or Phone
    }
}
