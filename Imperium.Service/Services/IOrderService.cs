using Imperium.Service.DTOs.Order;
using Imperium.Core.Enums;

namespace Imperium.Service.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto);
        Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status);
        Task<OrderDto> AddAdminNotesAsync(Guid id, string adminNotes);
    }
}