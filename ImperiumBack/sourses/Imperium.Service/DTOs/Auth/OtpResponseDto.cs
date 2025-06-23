using System;

namespace Imperium.Service.DTOs.Auth
{
    public class OtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingAttempts { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }
}
