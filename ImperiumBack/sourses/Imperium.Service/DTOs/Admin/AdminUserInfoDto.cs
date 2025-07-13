using System;

namespace Imperium.Service.DTOs.Admin
{
    /// <summary>
    /// DTO информации о пользователе-администраторе
    /// </summary>
    public class AdminUserInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
