using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Client;
using Imperium.Service.Services.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для управления клиентами
    /// </summary>
    [ApiController]
    [Route("api/admin/clients")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<AdminClientsController> _logger;

        public AdminClientsController(IClientService clientService, ILogger<AdminClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Поиск клиента по телефону
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> SearchClient([FromQuery] string phone)
        {
            try
            {
                var client = await _clientService.GetByPhoneAsync(phone);

                if (client == null)
                {
                    return NotFound(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });
                }

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = client
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching client by phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при поиске клиента"
                });
            }
        }

        /// <summary>
        /// Получить клиента по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> GetClientById(Guid id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);

                if (client == null)
                {
                    return NotFound(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });
                }

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = client
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при получении клиента"
                });
            }
        }

        /// <summary>
        /// Получить историю заказов клиента
        /// </summary>
        [HttpGet("{id}/orders")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientOrderHistoryDto>>>> GetClientOrders(Guid id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);
                if (client == null)
                {
                    return NotFound(new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });
                }

                var orders = await _clientService.GetClientOrdersHistoryAsync(client.Phone);

                return Ok(new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                {
                    Success = true,
                    Data = orders,
                    Message = $"Найдено {orders.Count()} заказов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for client {ClientId}", id);
                return StatusCode(500, new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении заказов клиента"
                });
            }
        }
    }
}
