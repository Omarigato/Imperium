using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Data.Repositories.Cart;
using Imperium.Data.Repositories.Order;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Email;
using Imperium.Service.Services.WhatsApp;
using Microsoft.Extensions.Logging;

namespace Imperium.Service.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdWithDetailsAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetWithDetailsAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
        {
            // Получаем товары из корзины
            var cartItems = await _cartRepository.GetByClientIdWithDetailsAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            // Создаем заказ
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
            var order = new Core.Models.Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                DeliveryAddressId = createOrderDto.DeliveryAddressId,
                Status = OrderStatus.Pending,
                Notes = createOrderDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Вычисляем общую сумму
            decimal totalAmount = 0;
            foreach (var cartItem in cartItems)
            {
                // Здесь нужно получить актуальную цену продукта
                totalAmount += cartItem.Quantity * 1000; // временно используем фиксированную цену
            }

            order.TotalAmount = totalAmount;
            var orderId = await _orderRepository.AddAsync(order);
            order.Id = orderId;

            // Создаем позиции заказа
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = 1000, // временно
                    TotalPrice = cartItem.Quantity * 1000,
                    SelectedColorId = cartItem.SelectedColorId,
                    SelectedSizeId = cartItem.SelectedSizeId,
                    ItemNotes = cartItem.Notes
                };

                await _orderItemRepository.AddAsync(orderItem);
            }

            // Очищаем корзину
            await _cartRepository.ClearUserCartAsync(userId);

            // Отправляем уведомления
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                await _emailService.SendOrderConfirmationAsync(user.Email, orderNumber);

                if (!string.IsNullOrEmpty(user.Phone))
                {
                    await _whatsAppService.SendOrderNotificationAsync(user.Phone, orderNumber, totalAmount);
                }

                await _whatsAppService.NotifyAdminNewOrderAsync(orderNumber, user.FullName);
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> AddAdminNotesAsync(Guid id, string adminNotes)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.AdminNotes = adminNotes;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            return _mapper.Map<OrderDto>(order);
        }
    }
}