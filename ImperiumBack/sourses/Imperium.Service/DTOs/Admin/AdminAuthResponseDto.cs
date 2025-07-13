using System;

namespace Imperium.Service.DTOs.Admin
{
    /// <summary>
    /// DTO ответа при успешной авторизации администратора
    /// </summary>
    public class AdminAuthResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
