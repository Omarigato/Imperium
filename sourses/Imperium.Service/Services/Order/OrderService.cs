using AutoMapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Email;
using Imperium.Service.Services.WhatsApp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
                return order != null ? _mapper.Map<OrderDto>(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
                return order != null ? _mapper.Map<OrderDto>(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Получаем корзину пользователя
                var cartItems = await _unitOfWork.Carts.GetByUserIdAsync(userId);
                if (!cartItems.Any())
                    throw new InvalidOperationException("Корзина пуста");

                // Создаем заказ
                var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();
                var order = new Core.Models.Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    DeliveryAddressId = createOrderDto.DeliveryAddressId,
                    Notes = createOrderDto.Notes,
                    Status = OrderStatus.Pending,
                    TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity),
                    DeliveryFee = 0 // Можно добавить логику расчета доставки
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Создаем позиции заказа
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

                    await _unitOfWork.OrderItems.AddAsync(orderItem);
                }

                // Очищаем корзину
                await _unitOfWork.Carts.ClearUserCartAsync(userId);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Отправляем уведомления
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    // Email уведомление
                    if (user.IsEmailVerified)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _emailService.SendOrderConfirmationAsync(user.Email, orderNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send order confirmation email to {Email}", user.Email);
                            }
                        });
                    }

                    // WhatsApp уведомление
                    if (user.IsPhoneVerified && !string.IsNullOrEmpty(user.Phone))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _whatsAppService.SendOrderNotificationAsync(user.Phone, orderNumber, order.TotalAmount);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send WhatsApp notification to {Phone}", user.Phone);
                            }
                        });
                    }

                    // Уведомление админа
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _whatsAppService.NotifyAdminNewOrderAsync(orderNumber, user.FullName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send admin notification for order {OrderNumber}", orderNumber);
                        }
                    });
                }

                var createdOrder = await _unitOfWork.Orders.GetWithDetailsAsync(order.Id);
                _logger.LogInformation("Order created successfully: {OrderNumber} for user {UserId}", orderNumber, userId);

                return _mapper.Map<OrderDto>(createdOrder!);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("Order not found");

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order status updated: {OrderId} to {Status}", id, status);

                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDto> AddAdminNotesAsync(Guid id, string adminNotes)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("Order not found");

                order.AdminNotes = adminNotes;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Admin notes added to order: {OrderId}", id);

                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding admin notes to order {OrderId}", id);
                throw;
            }
        }
    }
}