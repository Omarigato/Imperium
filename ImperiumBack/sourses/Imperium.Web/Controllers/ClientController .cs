using Imperium.Service.DTOs;
using Microsoft.AspNetCore.Mvc;
using Imperium.Service.DTOs.Client;
using Imperium.Service.DTOs.Address;
using Imperium.Service.Services.Client;

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
        /// Регистрация или авторизация клиента
        /// </summary>
        [HttpPost("register-or-login")]
        public async Task<ActionResult<ApiResponse<ClientAuthResponseDto>>> RegisterOrLogin([FromBody] ClientRegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<ClientAuthResponseDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных",
                        Data = new ClientAuthResponseDto { Success = false, Message = string.Join(", ", errors) }
                    });
                }

                var result = await _clientService.RegisterOrLoginAsync(registerDto);

                return Ok(new ApiResponse<ClientAuthResponseDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterOrLogin for phone {Phone}", registerDto.Phone);
                return StatusCode(500, new ApiResponse<ClientAuthResponseDto>
                {
                    Success = false,
                    Message = "Произошла ошибка при регистрации",
                    Data = new ClientAuthResponseDto { Success = false, Message = "Внутренняя ошибка сервера" }
                });
            }
        }

        /// <summary>
        /// Подтверждение OTP кода
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<ClientAuthResponseDto>>> VerifyOtp([FromBody] ClientVerifyOtpDto verifyDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<ClientAuthResponseDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных",
                        Data = new ClientAuthResponseDto { Success = false, Message = string.Join(", ", errors) }
                    });
                }

                var result = await _clientService.VerifyOtpAsync(verifyDto);

                return Ok(new ApiResponse<ClientAuthResponseDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOtp for phone {Phone}", verifyDto.Phone);
                return StatusCode(500, new ApiResponse<ClientAuthResponseDto>
                {
                    Success = false,
                    Message = "Произошла ошибка при проверке кода",
                    Data = new ClientAuthResponseDto { Success = false, Message = "Внутренняя ошибка сервера" }
                });
            }
        }

        /// <summary>
        /// Повторная отправка OTP кода
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<ActionResult<ApiResponse<ClientAuthResponseDto>>> ResendOtp([FromBody] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<ClientAuthResponseDto>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен",
                        Data = new ClientAuthResponseDto { Success = false, Message = "Номер телефона не указан" }
                    });

                var result = await _clientService.ResendOtpAsync(phone);

                return Ok(new ApiResponse<ClientAuthResponseDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResendOtp for phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<ClientAuthResponseDto>
                {
                    Success = false,
                    Message = "Произошла ошибка при повторной отправке кода",
                    Data = new ClientAuthResponseDto { Success = false, Message = "Внутренняя ошибка сервера" }
                });
            }
        }

        /// <summary>
        /// Найти клиента по номеру телефона
        /// </summary>
        [HttpGet("find-by-phone")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> FindByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен"
                    });

                var client = await _clientService.GetByPhoneAsync(phone);
                if (client == null)
                    return NotFound(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = client,
                    Message = "Клиент найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding client by phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при поиске клиента"
                });
            }
        }

        /// <summary>
        /// Получить информацию о клиенте по ID
        /// </summary>
        [HttpGet("{clientId}")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> GetById(Guid clientId)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(clientId);
                if (client == null)
                    return NotFound(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = client
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при получении информации о клиенте"
                });
            }
        }

        /// <summary>
        /// Обновить информацию о клиенте
        /// </summary>
        [HttpPut("{clientId}")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> UpdateClient(Guid clientId, [FromBody] UpdateClientDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var updatedClient = await _clientService.UpdateClientAsync(clientId, updateDto);
                if (updatedClient == null)
                    return NotFound(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = updatedClient,
                    Message = "Информация обновлена успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при обновлении информации о клиенте"
                });
            }
        }

        /// <summary>
        /// Получить адреса клиента
        /// </summary>
        [HttpGet("{clientId}/addresses")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddressDto>>>> GetClientAddresses(Guid clientId)
        {
            try
            {
                var addresses = await _clientService.GetClientAddressesAsync(clientId);

                return Ok(new ApiResponse<IEnumerable<AddressDto>>
                {
                    Success = true,
                    Data = addresses,
                    Message = $"Найдено {addresses.Count()} адресов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<IEnumerable<AddressDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении адресов"
                });
            }
        }

        /// <summary>
        /// Создать новый адрес для клиента
        /// </summary>
        [HttpPost("{clientId}/addresses")]
        public async Task<ActionResult<ApiResponse<AddressDto>>> CreateAddress(Guid clientId, [FromBody] CreateAddressDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<AddressDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var address = await _clientService.CreateAddressAsync(clientId, createDto);

                return CreatedAtAction(nameof(GetClientAddresses), new { clientId }, new ApiResponse<AddressDto>
                {
                    Success = true,
                    Data = address,
                    Message = "Адрес создан успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponse<AddressDto>
                {
                    Success = false,
                    Message = "Ошибка при создании адреса"
                });
            }
        }

        /// <summary>
        /// Обновить адрес клиента
        /// </summary>
        [HttpPut("{clientId}/addresses/{addressId}")]
        public async Task<ActionResult<ApiResponse<AddressDto>>> UpdateAddress(Guid clientId, Guid addressId, [FromBody] CreateAddressDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<AddressDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var address = await _clientService.UpdateAddressAsync(clientId, addressId, updateDto);
                if (address == null)
                    return NotFound(new ApiResponse<AddressDto>
                    {
                        Success = false,
                        Message = "Адрес не найден"
                    });

                return Ok(new ApiResponse<AddressDto>
                {
                    Success = true,
                    Data = address,
                    Message = "Адрес обновлен успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for client {ClientId}", addressId, clientId);
                return StatusCode(500, new ApiResponse<AddressDto>
                {
                    Success = false,
                    Message = "Ошибка при обновлении адреса"
                });
            }
        }

        /// <summary>
        /// Удалить адрес клиента
        /// </summary>
        [HttpDelete("{clientId}/addresses/{addressId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(Guid clientId, Guid addressId)
        {
            try
            {
                var success = await _clientService.DeleteAddressAsync(clientId, addressId);
                if (!success)
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Адрес не найден"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Адрес удален успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for client {ClientId}", addressId, clientId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении адреса"
                });
            }
        }

        /// <summary>
        /// Установить адрес как основной
        /// </summary>
        [HttpPut("{clientId}/addresses/{addressId}/set-default")]
        public async Task<ActionResult<ApiResponse<bool>>> SetDefaultAddress(Guid clientId, Guid addressId)
        {
            try
            {
                var success = await _clientService.SetDefaultAddressAsync(clientId, addressId);
                if (!success)
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Адрес не найден"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Основной адрес установлен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for client {ClientId}", addressId, clientId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при установке основного адреса"
                });
            }
        }

        /// <summary>
        /// Получить историю заказов клиента по номеру телефона
        /// </summary>
        [HttpGet("orders-history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientOrderHistoryDto>>>> GetOrdersHistory([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен"
                    });

                var orders = await _clientService.GetClientOrdersHistoryAsync(phone);

                return Ok(new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                {
                    Success = true,
                    Data = orders,
                    Message = $"Найдено {orders.Count()} заказов"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders history for phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<IEnumerable<ClientOrderHistoryDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении истории заказов"
                });
            }
        }

        /// <summary>
        /// Получить детали конкретного заказа клиента
        /// </summary>
        [HttpGet("order/{orderNumber}")]
        public async Task<ActionResult<ApiResponse<ClientOrderDetailsDto>>> GetOrderDetails(string orderNumber, [FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<ClientOrderDetailsDto>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен для доступа к заказу"
                    });

                var orderDetails = await _clientService.GetClientOrderDetailsAsync(orderNumber, phone);
                if (orderDetails == null)
                    return NotFound(new ApiResponse<ClientOrderDetailsDto>
                    {
                        Success = false,
                        Message = "Заказ не найден или не принадлежит указанному номеру телефона"
                    });

                return Ok(new ApiResponse<ClientOrderDetailsDto>
                {
                    Success = true,
                    Data = orderDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details {OrderNumber} for phone {Phone}", orderNumber, phone);
                return StatusCode(500, new ApiResponse<ClientOrderDetailsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении деталей заказа"
                });
            }
        }

        /// <summary>
        /// Проверить, есть ли клиент с таким номером телефона
        /// </summary>
        [HttpGet("check-phone")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPhoneExists([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен"
                    });

                var exists = await _clientService.PhoneExistsAsync(phone);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Номер найден в базе" : "Номер не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking phone existence {Phone}", phone);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при проверке номера телефона"
                });
            }
        }

        /// <summary>
        /// Получить статистику клиента (количество заказов, общая сумма)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<ClientStatsDto>>> GetClientStats([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<ClientStatsDto>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен"
                    });

                var stats = await _clientService.GetClientStatsAsync(phone);
                if (stats == null)
                    return NotFound(new ApiResponse<ClientStatsDto>
                    {
                        Success = false,
                        Message = "Клиент не найден"
                    });

                return Ok(new ApiResponse<ClientStatsDto>
                {
                    Success = true,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client stats for phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<ClientStatsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики клиента"
                });
            }
        }

        /// <summary>
        /// Получить список часто заказываемых товаров клиента
        /// </summary>
        [HttpGet("favorite-products")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientFavoriteProductDto>>>> GetFavoriteProducts([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new ApiResponse<IEnumerable<ClientFavoriteProductDto>>
                    {
                        Success = false,
                        Message = "Номер телефона обязателен"
                    });

                var favoriteProducts = await _clientService.GetClientFavoriteProductsAsync(phone);

                return Ok(new ApiResponse<IEnumerable<ClientFavoriteProductDto>>
                {
                    Success = true,
                    Data = favoriteProducts,
                    Message = $"Найдено {favoriteProducts.Count()} часто заказываемых товаров"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite products for phone {Phone}", phone);
                return StatusCode(500, new ApiResponse<IEnumerable<ClientFavoriteProductDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении списка товаров"
                });
            }
        }

        /// <summary>
        /// Обновить контактные данные клиента (используется при повторном заказе)
        /// </summary>
        [HttpPost("update-contact")]
        public async Task<ActionResult<ApiResponse<ClientInfoDto>>> UpdateContact([FromBody] UpdateContactDto updateContactDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<ClientInfoDto>
                    {
                        Success = false,
                        Message = "Проверьте правильность заполнения данных"
                    });
                }

                var client = await _clientService.UpdateClientContactAsync(updateContactDto);

                return Ok(new ApiResponse<ClientInfoDto>
                {
                    Success = true,
                    Data = client,
                    Message = "Контактные данные обновлены"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client contact for phone {Phone}", updateContactDto.Phone);
                return StatusCode(500, new ApiResponse<ClientInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при обновлении контактных данных"
                });
            }
        }
    }
}
