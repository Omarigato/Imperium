using AutoMapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Data.Repositories.Address;
using Imperium.Data.Repositories.Cart;
using Imperium.Data.Repositories.Client;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Order;
using Imperium.Data.Repositories.OrderItem;
using Imperium.Data.Repositories.Product;
using Imperium.Service.DTOs.Address;
using Imperium.Service.DTOs.Cart;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IClientRepository _clientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly ITelegramService _telegramService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICartRepository cartRepository,
            IClientRepository clientRepository,
            IAddressRepository addressRepository,
            IProductRepository productRepository,
            IFileRepository fileRepository,
            IDictionaryRepository dictionaryRepository,
            ITelegramService telegramService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _clientRepository = clientRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _dictionaryRepository = dictionaryRepository;
            _telegramService = telegramService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetClientOrdersAsync(Guid clientId)
        {
            try
            {
                var orders = await _orderRepository.GetByClientIdWithDetailsAsync(clientId);
                var result = new List<OrderSummaryDto>();

                foreach (var order in orders)
                {
                    var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);

                    result.Add(new OrderSummaryDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount,
                        DeliveryFee = order.DeliveryFee,
                        CreateDate = order.CreateDate,
                        ItemsCount = orderItems.Count(),
                        DeliveryAddress = order.DeliveryAddress != null
                            ? $"{order.DeliveryAddress.City}, {order.DeliveryAddress.Street}, {order.DeliveryAddress.HouseNumber}"
                            : null
                    });
                }

                return result.OrderByDescending(o => o.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for client {ClientId}", clientId);
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<OrderDetailsDto?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetWithDetailsAsync(orderId);
                if (order == null) return null;

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<OrderDetailsDto?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
                if (order == null) return null;

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by number {OrderNumber}", orderNumber);
                return null;
            }
        }

        public async Task<OrderDetailsDto> CreateOrderFromCartAsync(Guid clientId, CreateOrderFromCartDto createDto)
        {
            try
            {
                // Получаем клиента
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client == null)
                {
                    throw new InvalidOperationException("Клиент не найден");
                }

                // Получаем товары из корзины
                var cartItems = await _cartRepository.GetByClientIdAsync(clientId);

                // Фильтруем по выбранным товарам, если указаны
                if (createDto.SelectedCartItemIds?.Any() == true)
                {
                    cartItems = cartItems.Where(c => createDto.SelectedCartItemIds.Contains(c.Id));
                }

                if (!cartItems.Any())
                {
                    throw new InvalidOperationException("Корзина пуста");
                }

                // Получаем адрес доставки
                Address? deliveryAddress = null;
                if (createDto.DeliveryAddressId.HasValue)
                {
                    deliveryAddress = await _addressRepository.GetByIdAsync(createDto.DeliveryAddressId.Value);
                    if (deliveryAddress?.ClientId != clientId)
                    {
                        throw new InvalidOperationException("Адрес доставки не принадлежит клиенту");
                    }
                }

                // Создаем заказ
                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
                var totalAmount = 0m;

                var order = new Core.Models.Order
                {
                    OrderNumber = orderNumber,
                    ClientId = clientId,
                    DeliveryAddressId = deliveryAddress?.Id,
                    Status = OrderStatus.New,
                    Notes = createDto.Notes,
                    CreateDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _orderRepository.Insert(order);

                // Создаем элементы заказа
                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null || !product.IsAvailable)
                    {
                        continue; // Пропускаем недоступные товары
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * cartItem.Quantity,
                        SelectedColorId = cartItem.SelectedColorId,
                        SelectedSizeId = cartItem.SelectedSizeId,
                        ItemNotes = cartItem.Notes
                    };

                    await _orderItemRepository.Insert(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }

                // Рассчитываем доставку
                var deliveryFee = totalAmount >= 50000 ? 0 : 2000;

                // Обновляем заказ с итоговой суммой
                order.TotalAmount = totalAmount;
                order.DeliveryFee = deliveryFee;
                await _orderRepository.Update(order);

                // Очищаем корзину (выбранные товары)
                foreach (var cartItem in cartItems)
                {
                    cartItem.DeleteDate = DateTime.UtcNow;
                    await _cartRepository.Update(cartItem);
                }

                // Отправляем уведомление в Telegram
                await SendNewOrderNotification(order, client);

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from cart for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<OrderDetailsDto> CreateQuickOrderAsync(Guid clientId, QuickOrderDto quickOrderDto)
        {
            try
            {
                // Получаем клиента
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client == null)
                {
                    throw new InvalidOperationException("Клиент не найден");
                }

                // Проверяем товары
                var totalAmount = 0m;
                var validItems = new List<QuickOrderItemDto>();

                foreach (var item in quickOrderDto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null && product.IsAvailable)
                    {
                        validItems.Add(item);
                        totalAmount += product.Price * item.Quantity;
                    }
                }

                if (!validItems.Any())
                {
                    throw new InvalidOperationException("Нет доступных товаров для заказа");
                }

                // Получаем адрес доставки
                Address? deliveryAddress = null;
                if (quickOrderDto.DeliveryAddressId.HasValue)
                {
                    deliveryAddress = await _addressRepository.GetByIdAsync(quickOrderDto.DeliveryAddressId.Value);
                    if (deliveryAddress?.ClientId != clientId)
                    {
                        throw new InvalidOperationException("Адрес доставки не принадлежит клиенту");
                    }
                }

                // Создаем заказ
                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
                var deliveryFee = totalAmount >= 50000 ? 0 : 2000;

                var order = new Core.Models.Order
                {
                    OrderNumber = orderNumber,
                    ClientId = clientId,
                    DeliveryAddressId = deliveryAddress?.Id,
                    Status = OrderStatus.New,
                    TotalAmount = totalAmount,
                    DeliveryFee = deliveryFee,
                    Notes = quickOrderDto.Notes,
                    CreateDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _orderRepository.Insert(order);

                // Создаем элементы заказа
                foreach (var item in validItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product!.Price,
                        TotalPrice = product.Price * item.Quantity,
                        SelectedColorId = item.SelectedColorId,
                        SelectedSizeId = item.SelectedSizeId,
                        ItemNotes = item.Notes
                    };

                    await _orderItemRepository.Insert(orderItem);
                }

                // Отправляем уведомление в Telegram
                await SendNewOrderNotification(order, client);

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick order for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<OrderDetailsDto?> TakeOrderInWorkAsync(Guid orderId, Guid managerId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.Status != OrderStatus.New)
                {
                    return null;
                }

                order.Status = OrderStatus.Pending;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.Update(order);

                // Отправляем уведомление об изменении статуса
                await _telegramService.SendOrderStatusUpdateAsync(
                    order.OrderNumber,
                    OrderStatus.Pending.ToString(),
                    "Менеджер"); // TODO: получить имя менеджера по managerId

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking order {OrderId} in work by manager {ManagerId}", orderId, managerId);
                return null;
            }
        }

        public async Task<OrderDetailsDto?> ConfirmOrderAsync(Guid orderId, Guid managerId, string? adminNotes = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.Status == OrderStatus.Cancelled)
                {
                    return null;
                }

                order.Status = OrderStatus.Confirmed;
                order.AdminNotes = adminNotes;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.Update(order);

                // Отправляем уведомление об изменении статуса
                await _telegramService.SendOrderStatusUpdateAsync(
                    order.OrderNumber,
                    OrderStatus.Confirmed.ToString(),
                    "Менеджер"); // TODO: получить имя менеджера по managerId

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId} by manager {ManagerId}", orderId, managerId);
                return null;
            }
        }

        public async Task<OrderDetailsDto?> CancelOrderAsync(Guid orderId, Guid managerId, string? cancelReason = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.Status == OrderStatus.Confirmed)
                {
                    return null;
                }

                order.Status = OrderStatus.Cancelled;
                order.AdminNotes = cancelReason;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.Update(order);

                // Отправляем уведомление об изменении статуса
                await _telegramService.SendOrderStatusUpdateAsync(
                    order.OrderNumber,
                    OrderStatus.Cancelled.ToString(),
                    "Менеджер"); // TODO: получить имя менеджера по managerId

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} by manager {ManagerId}", orderId, managerId);
                return null;
            }
        }

        public async Task<OrderDetailsDto?> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateDto)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.Status == OrderStatus.Confirmed)
                {
                    return null;
                }

                // Обновляем заказ
                if (updateDto.DeliveryAddressId.HasValue)
                {
                    order.DeliveryAddressId = updateDto.DeliveryAddressId;
                }

                if (!string.IsNullOrEmpty(updateDto.Notes))
                {
                    order.Notes = updateDto.Notes;
                }

                if (!string.IsNullOrEmpty(updateDto.AdminNotes))
                {
                    order.AdminNotes = updateDto.AdminNotes;
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.Update(order);

                // Обновляем элементы заказа, если указаны
                if (updateDto.Items?.Any() == true)
                {
                    await UpdateOrderItems(orderId, updateDto.Items);

                    // Пересчитываем итоговую сумму
                    await RecalculateOrderTotal(orderId);
                }

                return await BuildOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAllWithDetailsAsync();
                var result = new List<OrderSummaryDto>();

                foreach (var order in orders)
                {
                    var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);

                    result.Add(new OrderSummaryDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount,
                        DeliveryFee = order.DeliveryFee,
                        CreateDate = order.CreateDate,
                        ItemsCount = orderItems.Count(),
                        ClientName = order.Client?.FullName,
                        ClientPhone = order.Client?.Phone,
                        DeliveryAddress = order.DeliveryAddress != null
                            ? $"{order.DeliveryAddress.City}, {order.DeliveryAddress.Street}"
                            : null
                    });
                }

                return result.OrderByDescending(o => o.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                var orders = await _orderRepository.GetByStatusAsync(status.ToString());
                var result = new List<OrderSummaryDto>();

                foreach (var order in orders)
                {
                    var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);
                    var client = await _clientRepository.GetByIdAsync(order.ClientId);

                    result.Add(new OrderSummaryDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount,
                        DeliveryFee = order.DeliveryFee,
                        CreateDate = order.CreateDate,
                        ItemsCount = orderItems.Count(),
                        ClientName = client?.FullName,
                        ClientPhone = client?.Phone
                    });
                }

                return result.OrderByDescending(o => o.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status {Status}", status);
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var allOrders = await _orderRepository.GetAllAsync();

                if (fromDate.HasValue)
                {
                    allOrders = allOrders.Where(o => o.CreateDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    allOrders = allOrders.Where(o => o.CreateDate <= toDate.Value);
                }

                var ordersList = allOrders.ToList();

                return new OrderStatisticsDto
                {
                    TotalOrders = ordersList.Count,
                    NewOrders = ordersList.Count(o => o.Status == OrderStatus.New),
                    PendingOrders = ordersList.Count(o => o.Status == OrderStatus.Pending),
                    ConfirmedOrders = ordersList.Count(o => o.Status == OrderStatus.Confirmed),
                    CancelledOrders = ordersList.Count(o => o.Status == OrderStatus.Cancelled),
                    TotalRevenue = ordersList.Where(o => o.Status == OrderStatus.Confirmed).Sum(o => o.TotalAmount),
                    AverageOrderValue = ordersList.Any() ? ordersList.Average(o => o.TotalAmount) : 0,
                    FromDate = fromDate,
                    ToDate = toDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return new OrderStatisticsDto();
            }
        }

        private async Task SendNewOrderNotification(Core.Models.Order order, Core.Models.Client client)
        {
            try
            {
                var adminPanelUrl = _configuration["AdminPanel:BaseUrl"];
                var orderUrl = $"{adminPanelUrl}/orders/{order.Id}";

                await _telegramService.SendNewOrderNotificationAsync(
                    order.OrderNumber,
                    client.FullName,
                    client.Phone,
                    order.TotalAmount + order.DeliveryFee,
                    orderUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending telegram notification for order {OrderNumber}", order.OrderNumber);
                // Не бросаем исключение, чтобы не сломать создание заказа
            }
        }

        private async Task<OrderDetailsDto> BuildOrderDetailsDto(Core.Models.Order order)
        {
            var client = await _clientRepository.GetByIdAsync(order.ClientId);
            var address = order.DeliveryAddressId.HasValue
                ? await _addressRepository.GetByIdAsync(order.DeliveryAddressId.Value)
                : null;
            var orderItems = await _orderItemRepository.GetByOrderIdWithDetailsAsync(order.Id);

            var orderItemDtos = new List<OrderItemDetailsDto>();

            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                var productImages = await _fileRepository.GetByProductIdAsync(item.ProductId);

                var selectedColor = item.SelectedColorId.HasValue
                    ? await _dictionaryRepository.GetByIdAsync(item.SelectedColorId.Value)
                    : null;

                var selectedSize = item.SelectedSizeId.HasValue
                    ? await _dictionaryRepository.GetByIdAsync(item.SelectedSizeId.Value)
                    : null;

                orderItemDtos.Add(new OrderItemDetailsDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product?.NameRu ?? "",
                    ProductCode = product?.Code ?? "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    SelectedColorName = selectedColor?.NameRu,
                    SelectedSizeName = selectedSize?.NameRu,
                    ItemNotes = item.ItemNotes,
                    ProductImages = productImages.Select(f => f.Url).ToList()
                });
            }

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DeliveryFee = order.DeliveryFee,
                Notes = order.Notes,
                AdminNotes = order.AdminNotes,
                CreateDate = order.CreateDate,
                UpdatedAt = order.UpdatedAt,
                ClientName = client?.FullName ?? "",
                ClientPhone = client?.Phone ?? "",
                DeliveryAddress = address != null ? _mapper.Map<AddressDto>(address) : null,
                Items = orderItemDtos
            };
        }

        private async Task UpdateOrderItems(Guid orderId, List<UpdateOrderItemDto> items)
        {
            // Получаем существующие элементы заказа
            var existingItems = await _orderItemRepository.GetByOrderIdAsync(orderId);

            foreach (var updateItem in items)
            {
                var existingItem = existingItems.FirstOrDefault(ei => ei.Id == updateItem.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity = updateItem.Quantity;
                    existingItem.ItemNotes = updateItem.Notes;
                    existingItem.TotalPrice = existingItem.UnitPrice * updateItem.Quantity;

                    await _orderItemRepository.Update(existingItem);
                }
            }
        }

        private async Task RecalculateOrderTotal(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId);

            if (order != null)
            {
                var totalAmount = orderItems.Sum(oi => oi.TotalPrice);
                var deliveryFee = totalAmount >= 50000 ? 0 : 2000;

                order.TotalAmount = totalAmount;
                order.DeliveryFee = deliveryFee;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.Update(order);
            }
        }
    }
}