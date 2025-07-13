using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Imperium.Core.Enums;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Order;
using Imperium.Service.DTOs.Cart;
using Imperium.Service.Services.Order;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с заказами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Получить заказы клиента
        /// </summary>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetClientOrders(Guid clientId)
        {
            try
            {
                var orders = await _orderService.GetClientOrdersAsync(clientId);

                return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = true,
                    Data = orders,
                    Message = $"Найдено {orders.Count()} заказов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении заказов"
                });
            }
        }

        /// <summary>
        /// Получить детали заказа по ID
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> GetOrderById(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Заказ не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении заказа"
                });
            }
        }

        /// <summary>
        /// Получить детали заказа по номеру
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _orderService.GetOrderByNumberAsync(orderNumber);

                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Заказ не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by number {OrderNumber}", orderNumber);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении заказа"
                });
            }
        }

        /// <summary>
        /// Создать заказ из корзины
        /// </summary>
        [HttpPost("client/{clientId}/from-cart")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> CreateOrderFromCart(Guid clientId, [FromBody] CreateOrderFromCartDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var order = await _orderService.CreateOrderFromCartAsync(clientId, createDto);

                return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = $"Заказ {order.OrderNumber} создан успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from cart for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при создании заказа"
                });
            }
        }

        /// <summary>
        /// Создать быстрый заказ
        /// </summary>
        [HttpPost("client/{clientId}/quick")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> CreateQuickOrder(Guid clientId, [FromBody] QuickOrderDto quickOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var order = await _orderService.CreateQuickOrderAsync(clientId, quickOrderDto);

                return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = $"Заказ {order.OrderNumber} создан успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick order for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при создании заказа"
                });
            }
        }

        // === АДМИНСКИЕ МЕТОДЫ ===

        /// <summary>
        /// Получить все заказы (для админов)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();

                return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = true,
                    Data = orders,
                    Message = $"Найдено {orders.Count()} заказов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении заказов"
                });
            }
        }

        /// <summary>
        /// Получить заказы по статусу
        /// </summary>
        [HttpGet("admin/status/{status}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetOrdersByStatus(OrderStatus status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);

                return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = true,
                    Data = orders,
                    Message = $"Найдено {orders.Count()} заказов со статусом {status}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status {Status}", status);
                return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении заказов"
                });
            }
        }

        /// <summary>
        /// Взять заказ в работу
        /// </summary>
        [HttpPut("admin/{orderId}/take-in-work")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> TakeOrderInWork(Guid orderId)
        {
            try
            {
                // TODO: Получить ID текущего пользователя из токена
                var managerId = Guid.NewGuid(); // Временно

                var order = await _orderService.TakeOrderInWorkAsync(orderId, managerId);

                if (order == null)
                {
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Нельзя взять этот заказ в работу или он не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Заказ взят в работу"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking order {OrderId} in work", orderId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при взятии заказа в работу"
                });
            }
        }

        /// <summary>
        /// Подтвердить заказ
        /// </summary>
        [HttpPut("admin/{orderId}/confirm")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> ConfirmOrder(Guid orderId, [FromBody] AdminOrderActionDto? actionDto = null)
        {
            try
            {
                // TODO: Получить ID текущего пользователя из токена
                var managerId = Guid.NewGuid(); // Временно

                var order = await _orderService.ConfirmOrderAsync(orderId, managerId, actionDto?.Notes);

                if (order == null)
                {
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Нельзя подтвердить этот заказ или он не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Заказ подтвержден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при подтверждении заказа"
                });
            }
        }

        /// <summary>
        /// Отменить заказ
        /// </summary>
        [HttpPut("admin/{orderId}/cancel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> CancelOrder(Guid orderId, [FromBody] AdminOrderActionDto? actionDto = null)
        {
            try
            {
                // TODO: Получить ID текущего пользователя из токена
                var managerId = Guid.NewGuid(); // Временно

                var order = await _orderService.CancelOrderAsync(orderId, managerId, actionDto?.Reason);

                if (order == null)
                {
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Нельзя отменить этот заказ или он не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Заказ отменен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при отмене заказа"
                });
            }
        }

        /// <summary>
        /// Обновить заказ
        /// </summary>
        [HttpPut("admin/{orderId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDto>>> UpdateOrder(Guid orderId, [FromBody] UpdateOrderDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var order = await _orderService.UpdateOrderAsync(orderId, updateDto);

                if (order == null)
                {
                    return BadRequest(new ApiResponse<OrderDetailsDto>
                    {
                        Success = false,
                        Message = "Нельзя обновить этот заказ или он не найден"
                    });
                }

                return Ok(new ApiResponse<OrderDetailsDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Заказ обновлен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<OrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при обновлении заказа"
                });
            }
        }

        /// <summary>
        /// Получить статистику заказов
        /// </summary>
        [HttpGet("admin/statistics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderStatisticsDto>>> GetOrderStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync(fromDate, toDate);

                return Ok(new ApiResponse<OrderStatisticsDto>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Статистика заказов получена"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return StatusCode(500, new ApiResponse<OrderStatisticsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики"
                });
            }
        }
    }
}