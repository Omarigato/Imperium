using Imperium.Core.Enums;
using Imperium.Service.DTOs.Cart;
using Imperium.Service.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Order
{
    public interface IOrderService
    {
        // Получение заказов
        Task<IEnumerable<OrderSummaryDto>> GetClientOrdersAsync(Guid clientId);
        Task<OrderDetailsDto?> GetOrderByIdAsync(Guid orderId);
        Task<OrderDetailsDto?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status);

        // Создание заказов
        Task<OrderDetailsDto> CreateOrderFromCartAsync(Guid clientId, CreateOrderFromCartDto createDto);
        Task<OrderDetailsDto> CreateQuickOrderAsync(Guid clientId, QuickOrderDto quickOrderDto);

        // Управление заказами (для админов/менеджеров)
        Task<OrderDetailsDto?> TakeOrderInWorkAsync(Guid orderId, Guid managerId);
        Task<OrderDetailsDto?> ConfirmOrderAsync(Guid orderId, Guid managerId, string? adminNotes = null);
        Task<OrderDetailsDto?> CancelOrderAsync(Guid orderId, Guid managerId, string? cancelReason = null);
        Task<OrderDetailsDto?> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateDto);

        // Статистика
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}