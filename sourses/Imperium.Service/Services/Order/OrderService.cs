using AutoMapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Email;
using Imperium.Service.Services.WhatsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _mapper = mapper;
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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var cartItems = await _cartRepository.GetByUserIdAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            // Создаем заказ
            var order = new Core.Models.Order
            {
                OrderNumber = await _orderRepository.GenerateOrderNumberAsync(),
                UserId = userId,
                DeliveryAddressId = createOrderDto.DeliveryAddressId,
                Status = OrderStatus.Pending,
                Notes = createOrderDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Рассчитываем общую сумму
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || !product.IsAvailable)
                    continue;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * cartItem.Quantity,
                    SelectedColorId = cartItem.SelectedColorId,
                    SelectedSizeId = cartItem.SelectedSizeId,
                    ItemNotes = cartItem.Notes
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }

            order.TotalAmount = totalAmount;

            // Сохраняем заказ
            var orderId = await _orderRepository.AddAsync(order);
            order.Id = orderId;

            // Сохраняем элементы заказа
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = orderId;
                await _orderItemRepository.AddAsync(orderItem);
            }

            // Очищаем корзину
            await _cartRepository.ClearUserCartAsync(userId);

            // Отправляем уведомления
            try
            {
                await _emailService.SendOrderConfirmationAsync(user.Email, order.OrderNumber);

                if (!string.IsNullOrEmpty(user.Phone))
                {
                    await _whatsAppService.SendOrderNotificationAsync(user.Phone, order.OrderNumber, totalAmount);
                }

                await _whatsAppService.NotifyAdminNewOrderAsync(order.OrderNumber, user.FullName);
            }
            catch
            {
                // Игнорируем ошибки отправки уведомлений
            }

            var createdOrder = await _orderRepository.GetWithDetailsAsync(orderId);
            return _mapper.Map<OrderDto>(createdOrder!);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            var updatedOrder = await _orderRepository.GetWithDetailsAsync(id);
            return _mapper.Map<OrderDto>(updatedOrder!);
        }

        public async Task<OrderDto> AddAdminNotesAsync(Guid id, string adminNotes)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.AdminNotes = adminNotes;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            var updatedOrder = await _orderRepository.GetWithDetailsAsync(id);
            return _mapper.Map<OrderDto>(updatedOrder!);
        }
    }
}