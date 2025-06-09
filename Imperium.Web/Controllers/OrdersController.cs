using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Imperium.Core.Enums;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Order;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (order.User.Id != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(order);
        }

        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (order.User.Id != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] OrderStatus status)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, status);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/admin-notes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> AddAdminNotes(Guid id, [FromBody] string adminNotes)
        {
            try
            {
                var order = await _orderService.AddAdminNotesAsync(id, adminNotes);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}