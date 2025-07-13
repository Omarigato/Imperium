using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с клиентами без авторизации
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Найти клиента по номеру телефона
        /// </summary>
        [HttpGet("find-by-phone")]
        public async Task<ActionResult<ClientInfoDto>> FindByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });

                var client = await _clientService.GetByPhoneAsync(phone);
                if (client == null)
                    return NotFound(new { success = false, message = "Клиент не найден" });

                return Ok(new
                {
                    success = true,
                    data = client,
                    message = "Клиент найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding client by phone {Phone}", phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при поиске клиента"
                });
            }
        }

        /// <summary>
        /// Получить информацию о клиенте по ID
        /// </summary>
        [HttpGet("{clientId}")]
        public async Task<ActionResult<ClientInfoDto>> GetById(Guid clientId)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(clientId);
                if (client == null)
                    return NotFound(new { success = false, message = "Клиент не найден" });

                return Ok(new
                {
                    success = true,
                    data = client
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", clientId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при получении информации о клиенте"
                });
            }
        }

        /// <summary>
        /// Обновить информацию о клиенте
        /// </summary>
        [HttpPut("{clientId}")]
        public async Task<ActionResult<ClientInfoDto>> UpdateClient(Guid clientId, [FromBody] UpdateClientDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Проверьте правильность заполнения данных",
                        errors
                    });
                }

                var updatedClient = await _clientService.UpdateClientAsync(clientId, updateDto);
                if (updatedClient == null)
                    return NotFound(new { success = false, message = "Клиент не найден" });

                return Ok(new
                {
                    success = true,
                    data = updatedClient,
                    message = "Информация обновлена успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", clientId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при обновлении информации о клиенте"
                });
            }
        }

        /// <summary>
        /// Получить историю заказов клиента по номеру телефона
        /// </summary>
        [HttpGet("orders-history")]
        public async Task<ActionResult<IEnumerable<ClientOrderHistoryDto>>> GetOrdersHistory([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });

                var orders = await _clientService.GetClientOrdersHistoryAsync(phone);

                return Ok(new
                {
                    success = true,
                    data = orders,
                    count = orders.Count(),
                    message = $"Найдено {orders.Count()} заказов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders history for phone {Phone}", phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при получении истории заказов"
                });
            }
        }

        /// <summary>
        /// Получить детали конкретного заказа клиента
        /// </summary>
        [HttpGet("order/{orderNumber}")]
        public async Task<ActionResult<ClientOrderDetailsDto>> GetOrderDetails(string orderNumber, [FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен для доступа к заказу" });

                var orderDetails = await _clientService.GetClientOrderDetailsAsync(orderNumber, phone);
                if (orderDetails == null)
                    return NotFound(new { success = false, message = "Заказ не найден или не принадлежит указанному номеру телефона" });

                return Ok(new
                {
                    success = true,
                    data = orderDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details {OrderNumber} for phone {Phone}", orderNumber, phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при получении деталей заказа"
                });
            }
        }

        /// <summary>
        /// Проверить, есть ли клиент с таким номером телефона
        /// </summary>
        [HttpGet("check-phone")]
        public async Task<ActionResult> CheckPhoneExists([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });

                var exists = await _clientService.PhoneExistsAsync(phone);

                return Ok(new
                {
                    success = true,
                    exists = exists,
                    message = exists ? "Номер найден в базе" : "Номер не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking phone existence {Phone}", phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при проверке номера телефона"
                });
            }
        }

        /// <summary>
        /// Получить статистику клиента (количество заказов, общая сумма)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ClientStatsDto>> GetClientStats([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });

                var stats = await _clientService.GetClientStatsAsync(phone);
                if (stats == null)
                    return NotFound(new { success = false, message = "Клиент не найден" });

                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client stats for phone {Phone}", phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при получении статистики клиента"
                });
            }
        }

        /// <summary>
        /// Получить список часто заказываемых товаров клиента
        /// </summary>
        [HttpGet("favorite-products")]
        public async Task<ActionResult<IEnumerable<ClientFavoriteProductDto>>> GetFavoriteProducts([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });

                var favoriteProducts = await _clientService.GetClientFavoriteProductsAsync(phone);

                return Ok(new
                {
                    success = true,
                    data = favoriteProducts,
                    message = $"Найдено {favoriteProducts.Count()} часто заказываемых товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite products for phone {Phone}", phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при получении списка товаров"
                });
            }
        }

        /// <summary>
        /// Обновить контактные данные клиента (используется при повторном заказе)
        /// </summary>
        [HttpPost("update-contact")]
        public async Task<ActionResult<ClientInfoDto>> UpdateContact([FromBody] UpdateContactDto updateContactDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Проверьте правильность заполнения данных",
                        errors
                    });
                }

                var client = await _clientService.UpdateClientContactAsync(updateContactDto);

                return Ok(new
                {
                    success = true,
                    data = client,
                    message = "Контактные данные обновлены"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client contact for phone {Phone}", updateContactDto.Phone);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при обновлении контактных данных"
                });
            }
        }
    }
