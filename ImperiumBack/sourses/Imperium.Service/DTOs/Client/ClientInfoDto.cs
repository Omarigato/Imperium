using System;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO информации о клиенте
    /// </summary>
    public class ClientInfoDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
