using System;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO ответа при регистрации/авторизации клиента
    /// </summary>
    public class ClientAuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresOtp { get; set; }
        public ClientInfoDto? Client { get; set; }
        public int? RemainingAttempts { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }
}
