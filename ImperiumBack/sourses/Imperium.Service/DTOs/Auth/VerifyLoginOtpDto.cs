using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Auth
{
    public class VerifyLoginOtpDto
    {
        [Required]
        public string Contact { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;
    }
}
