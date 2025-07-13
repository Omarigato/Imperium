using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO статистики заказов
    /// </summary>
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CancelledOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Вычисляемые поля
        public double ConfirmationRate => TotalOrders > 0 ? (double)ConfirmedOrders / TotalOrders * 100 : 0;
        public double CancellationRate => TotalOrders > 0 ? (double)CancelledOrders / TotalOrders * 100 : 0;
    }
}
