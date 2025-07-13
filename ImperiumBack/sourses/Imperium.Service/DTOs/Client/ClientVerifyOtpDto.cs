using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO для верификации клиента по OTP
    /// </summary>
    public class ClientVerifyOtpDto
    {
        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;
    }
}
