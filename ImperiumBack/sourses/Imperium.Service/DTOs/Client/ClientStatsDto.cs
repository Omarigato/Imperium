using System;

namespace Imperium.Service.DTOs.Client
{
    // <summary>
    /// DTO статистики клиента
    /// </summary>
    public class ClientStatsDto
    {
        public int TotalOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }
}
