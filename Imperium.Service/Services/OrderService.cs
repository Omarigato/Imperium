using AutoMapper;
using Imperium.Core.Models;
using Imperium.Core.Enums;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Order;

namespace Imperium.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
        {
            var cartItems = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                var order = _mapper.Map<Order>(createOrderDto);
                order.UserId = userId;
                order.OrderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();
                order.TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
                
                await _unitOfWork.Orders.AddAsync(order);
                
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price,
                        TotalPrice = cartItem.Product.Price * cartItem.Quantity,
                        SelectedColorId = cartItem.SelectedColorId,
                        SelectedSizeId = cartItem.SelectedSizeId,
                        ItemNotes = cartItem.Notes
                    };
                    
                    await _unitOfWork.Orders.AddAsync(orderItem);
                }
                
                await _unitOfWork.Carts.ClearUserCartAsync(userId);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                var createdOrder = await _unitOfWork.Orders.GetWithDetailsAsync(order.Id);
                return _mapper.Map<OrderDto>(createdOrder!);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            var updatedOrder = await _unitOfWork.Orders.GetWithDetailsAsync(id);
            return _mapper.Map<OrderDto>(updatedOrder!);
        }

        public async Task<OrderDto> AddAdminNotesAsync(Guid id, string adminNotes)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.AdminNotes = adminNotes;
            order.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            var updatedOrder = await _unitOfWork.Orders.GetWithDetailsAsync(id);
            return _mapper.Map<OrderDto>(updatedOrder!);
        }
    }
}