using AutoMapper;
using Imperium.Core;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories.Address;
using Imperium.Data.Repositories.Client;
using Imperium.Data.Repositories.Favorite;
using Imperium.Data.Repositories.Order;
using Imperium.Data.Repositories.OrderItem;
using Imperium.Data.Repositories.Verification;
using Imperium.Service.DTOs.Address;
using Imperium.Service.DTOs.Client;
using Imperium.Service.DTOs.Order;
using Imperium.Service.Services.Validation;
using Imperium.Service.Services.WhatsApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Client
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IVerificationRepository _verificationRepository;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IPhoneValidationService _phoneValidationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientService> _logger;
        private readonly bool _checkOtpEnabled = false;

        public ClientService(
            IClientRepository clientRepository,
            IAddressRepository addressRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IVerificationRepository verificationRepository,
            IFavoriteRepository favoriteRepository,
            IWhatsAppService whatsAppService,
            IPhoneValidationService phoneValidationService,
            IMapper mapper,
            ILogger<ClientService> logger)
        {
            _clientRepository = clientRepository;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _verificationRepository = verificationRepository;
            _favoriteRepository = favoriteRepository;
            _whatsAppService = whatsAppService;
            _phoneValidationService = phoneValidationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClientAuthResponseDto> RegisterOrLoginAsync(ClientRegisterDto registerDto)
        {
            try
            {
                // Форматируем номер телефона
                var formattedPhone = _phoneValidationService.FormatPhoneNumber(registerDto.Phone);

                if (!_phoneValidationService.IsValidKazakhstanPhoneNumber(formattedPhone))
                {
                    return new ClientAuthResponseDto
                    {
                        Success = false,
                        Message = "Неверный формат номера телефона",
                        RequiresOtp = false
                    };
                }

                // Проверяем, существует ли клиент
                var existingClient = await _clientRepository.GetByPhoneAsync(formattedPhone);

                if (existingClient != null)
                {
                    // Обновляем ФИО если изменилось
                    if (existingClient.FullName != registerDto.FullName)
                    {
                        existingClient.FullName = registerDto.FullName;
                        existingClient.UpdateDate = DateTime.UtcNow;
                        await _clientRepository.Update(existingClient);
                    }
                }
                else
                {
                    // Создаем нового клиента
                    existingClient = new Core.Models.Client
                    {
                        FullName = registerDto.FullName,
                        Phone = formattedPhone,
                        IsPhoneVerified = false,
                        CreateDate = DateTime.UtcNow,
                        UpdateDate = DateTime.UtcNow
                    };

                    await _clientRepository.Insert(existingClient);
                }

                // Если OTP проверка отключена, сразу верифицируем и возвращаем клиента
                if (!_checkOtpEnabled)
                {
                    if (!existingClient.IsPhoneVerified)
                    {
                        await _clientRepository.VerifyPhoneAsync(existingClient.Id);
                        existingClient.IsPhoneVerified = true;
                    }

                    var clientInfo = await GetClientWithDetailsAsync(existingClient.Id);
                    return new ClientAuthResponseDto
                    {
                        Success = true,
                        Message = "Авторизация прошла успешно",
                        RequiresOtp = false,
                        Client = clientInfo
                    };
                }

                // Если клиент уже верифицирован, не отправляем OTP
                if (existingClient.IsPhoneVerified)
                {
                    var clientInfo = await GetClientWithDetailsAsync(existingClient.Id);
                    return new ClientAuthResponseDto
                    {
                        Success = true,
                        Message = "Добро пожаловать!",
                        RequiresOtp = false,
                        Client = clientInfo
                    };
                }

                // Отправляем OTP код
                var otpResult = await SendOtpCodeAsync(existingClient.Id, formattedPhone);

                return new ClientAuthResponseDto
                {
                    Success = otpResult.Success,
                    Message = otpResult.Message,
                    RequiresOtp = true,
                    RemainingAttempts = otpResult.RemainingAttempts,
                    NextRetryAt = otpResult.NextRetryAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterOrLoginAsync for phone {Phone}", registerDto.Phone);
                return new ClientAuthResponseDto
                {
                    Success = false,
                    Message = "Произошла ошибка при регистрации",
                    RequiresOtp = false
                };
            }
        }

        public async Task<ClientAuthResponseDto> VerifyOtpAsync(ClientVerifyOtpDto verifyDto)
        {
            try
            {
                var formattedPhone = _phoneValidationService.FormatPhoneNumber(verifyDto.Phone);
                var client = await _clientRepository.GetByPhoneAsync(formattedPhone);

                if (client == null)
                {
                    return new ClientAuthResponseDto
                    {
                        Success = false,
                        Message = "Клиент не найден",
                        RequiresOtp = false
                    };
                }

                // Проверяем OTP код
                var verification = await _verificationRepository.GetActiveByClientAndContactAsync(
                    client.Id, formattedPhone, VerificationType.Phone.ToString());

                if (verification == null || verification.Code != verifyDto.OtpCode)
                {
                    return new ClientAuthResponseDto
                    {
                        Success = false,
                        Message = "Неверный или истекший код",
                        RequiresOtp = true,
                        RemainingAttempts = Math.Max(0, Core.Constants.OTP_COUNT - (verification?.AttemptCount ?? 0))
                    };
                }

                // Верифицируем клиента
                await _clientRepository.VerifyPhoneAsync(client.Id);
                await _verificationRepository.MarkAsUsedAsync(verification.Id);

                var clientInfo = await GetClientWithDetailsAsync(client.Id);

                return new ClientAuthResponseDto
                {
                    Success = true,
                    Message = "Номер телефона подтвержден!",
                    RequiresOtp = false,
                    Client = clientInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOtpAsync for phone {Phone}", verifyDto.Phone);
                return new ClientAuthResponseDto
                {
                    Success = false,
                    Message = "Произошла ошибка при проверке кода",
                    RequiresOtp = false
                };
            }
        }

        public async Task<ClientAuthResponseDto> ResendOtpAsync(string phone)
        {
            try
            {
                var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
                var client = await _clientRepository.GetByPhoneAsync(formattedPhone);

                if (client == null)
                {
                    return new ClientAuthResponseDto
                    {
                        Success = false,
                        Message = "Клиент не найден",
                        RequiresOtp = false
                    };
                }

                var otpResult = await SendOtpCodeAsync(client.Id, formattedPhone);

                return new ClientAuthResponseDto
                {
                    Success = otpResult.Success,
                    Message = otpResult.Message,
                    RequiresOtp = true,
                    RemainingAttempts = otpResult.RemainingAttempts,
                    NextRetryAt = otpResult.NextRetryAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResendOtpAsync for phone {Phone}", phone);
                return new ClientAuthResponseDto
                {
                    Success = false,
                    Message = "Произошла ошибка при повторной отправке кода",
                    RequiresOtp = false
                };
            }
        }

        private async Task<(bool Success, string Message, int? RemainingAttempts, DateTime? NextRetryAt)> SendOtpCodeAsync(Guid clientId, string phone)
        {
            try
            {
                // Проверяем последние верификации
                var recentVerifications = await _verificationRepository.GetByContactAndTypeAsync(
                    phone, VerificationType.Phone.ToString());

                var lastVerification = recentVerifications
                    .Where(v => v.ClientId == clientId)
                    .OrderByDescending(v => v.CreateDate)
                    .FirstOrDefault();

                // Проверяем кулдаун
                if (lastVerification != null &&
                    lastVerification.CreateDate > DateTime.UtcNow.AddMinutes(-Constants.OTP_RESEND_COOLDOWN_MINUTES))
                {
                    return (false, $"Повторная отправка доступна через {Constants.OTP_RESEND_COOLDOWN_MINUTES} минуту",
                           null, lastVerification.CreateDate.AddMinutes(Constants.OTP_RESEND_COOLDOWN_MINUTES));
                }

                // Проверяем максимальное количество попыток
                if (lastVerification != null && lastVerification.AttemptCount >= Constants.OTP_COUNT)
                {
                    return (false, "Превышено максимальное количество попыток", 0, null);
                }

                // Генерируем и отправляем код
                var otpCode = GenerateOtpCode();
                var verification = new Core.Models.Verification
                {
                    ClientId = clientId,
                    Type = VerificationType.Phone,
                    Contact = phone,
                    Code = otpCode,
                    ExpireDate = DateTime.UtcNow.AddMinutes(Constants.OTP_EXPIRY_MINUTES),
                    AttemptCount = (lastVerification?.AttemptCount ?? 0) + 1,
                    CreateDate = DateTime.UtcNow
                };

                await _verificationRepository.Insert(verification);

                var sent = await _whatsAppService.SendVerificationCodeAsync(phone, otpCode);

                if (!sent)
                {
                    return (false, "Не удалось отправить код", null, null);
                }

                return (true, "Код отправлен в WhatsApp",
                       Constants.OTP_COUNT - verification.AttemptCount, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Phone}", phone);
                return (false, "Ошибка при отправке кода", null, null);
            }
        }

        public async Task<ClientInfoDto?> GetByIdAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null) return null;

            return await GetClientWithDetailsAsync(clientId);
        }

        public async Task<ClientInfoDto?> GetByPhoneAsync(string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);
            if (client == null) return null;

            return await GetClientWithDetailsAsync(client.Id);
        }

        private async Task<ClientInfoDto> GetClientWithDetailsAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            var addresses = await _addressRepository.GetByClientIdAsync(clientId);
            var stats = await GetClientStatsByIdAsync(clientId);

            var clientDto = _mapper.Map<ClientInfoDto>(client);
            clientDto.Addresses = _mapper.Map<List<AddressDto>>(addresses);
            clientDto.Stats = stats;

            return clientDto;
        }

        private async Task<ClientStatsDto> GetClientStatsByIdAsync(Guid clientId)
        {
            var orders = await _orderRepository.GetByClientIdAsync(clientId);

            return new ClientStatsDto
            {
                TotalOrders = orders.Count(),
                ConfirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                TotalSpent = orders.Where(o => o.Status == OrderStatus.Confirmed).Sum(o => o.TotalAmount),
                LastOrderDate = orders.Any() ? orders.Max(o => o.CreateDate) : null
            };
        }

        public async Task<ClientInfoDto?> UpdateClientAsync(Guid clientId, UpdateClientDto updateDto)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null) return null;

            var formattedPhone = _phoneValidationService.FormatPhoneNumber(updateDto.Phone);

            // Проверяем, не занят ли новый номер другим клиентом
            if (formattedPhone != client.Phone)
            {
                var existingClient = await _clientRepository.GetByPhoneAsync(formattedPhone);
                if (existingClient != null)
                {
                    throw new InvalidOperationException("Номер телефона уже используется другим клиентом");
                }

                client.Phone = formattedPhone;
                client.IsPhoneVerified = false; // Требуется повторная верификация
            }

            client.FullName = updateDto.FullName;
            client.UpdateDate = DateTime.UtcNow;

            await _clientRepository.Update(client);

            return await GetClientWithDetailsAsync(clientId);
        }

        public async Task<ClientInfoDto?> UpdateClientContactAsync(UpdateContactDto updateContactDto)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(updateContactDto.Phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);

            if (client == null)
            {
                // Создаем нового клиента
                client = new Core.Models.Client
                {
                    FullName = updateContactDto.FullName,
                    Phone = formattedPhone,
                    IsPhoneVerified = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _clientRepository.Insert(client);
            }
            else
            {
                // Обновляем существующего
                client.FullName = updateContactDto.FullName;
                client.UpdateDate = DateTime.UtcNow;
                await _clientRepository.Update(client);
            }

            return await GetClientWithDetailsAsync(client.Id);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            return await _clientRepository.PhoneExistsAsync(formattedPhone);
        }

        // Методы для работы с адресами
        public async Task<IEnumerable<AddressDto>> GetClientAddressesAsync(Guid clientId)
        {
            var addresses = await _addressRepository.GetByClientIdAsync(clientId);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<AddressDto> CreateAddressAsync(Guid clientId, CreateAddressDto createDto)
        {
            var address = _mapper.Map<Address>(createDto);
            address.ClientId = clientId;
            address.CreateDate = DateTime.UtcNow;

            // Если это первый адрес или указан как default
            if (createDto.IsDefault)
            {
                await _addressRepository.UnsetDefaultAddressesAsync(clientId);
                address.IsDefault = true;
            }
            else
            {
                var existingAddresses = await _addressRepository.GetByClientIdAsync(clientId);
                if (!existingAddresses.Any())
                {
                    address.IsDefault = true;
                }
            }

            await _addressRepository.Insert(address);
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto?> UpdateAddressAsync(Guid clientId, Guid addressId, CreateAddressDto updateDto)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.ClientId != clientId) return null;

            _mapper.Map(updateDto, address);

            if (updateDto.IsDefault && !address.IsDefault)
            {
                await _addressRepository.UnsetDefaultAddressesAsync(clientId);
                address.IsDefault = true;
            }

            await _addressRepository.Update(address);
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<bool> DeleteAddressAsync(Guid clientId, Guid addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.ClientId != clientId) return false;

            address.DeleteDate = DateTime.UtcNow;
            await _addressRepository.Update(address);

            // Если удаляем default адрес, устанавливаем другой как default
            if (address.IsDefault)
            {
                var otherAddresses = await _addressRepository.GetByClientIdAsync(clientId);
                var firstOther = otherAddresses.FirstOrDefault();
                if (firstOther != null)
                {
                    await _addressRepository.SetDefaultAddressAsync(clientId, firstOther.Id);
                }
            }

            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(Guid clientId, Guid addressId)
        {
            return await _addressRepository.SetDefaultAddressAsync(clientId, addressId);
        }

        // Методы для истории и статистики
        public async Task<IEnumerable<ClientOrderHistoryDto>> GetClientOrdersHistoryAsync(string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);
            if (client == null) return new List<ClientOrderHistoryDto>();

            var orders = await _orderRepository.GetByClientIdWithDetailsAsync(client.Id);
            var result = new List<ClientOrderHistoryDto>();

            foreach (var order in orders)
            {
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);

                result.Add(new ClientOrderHistoryDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    DeliveryFee = order.DeliveryFee,
                    CreateDate = order.CreateDate,
                    ItemsCount = orderItems.Count(),
                    DeliveryAddress = order.DeliveryAddress?.City + ", " + order.DeliveryAddress?.Street
                });
            }

            return result.OrderByDescending(o => o.CreateDate);
        }

        public async Task<ClientOrderDetailsDto?> GetClientOrderDetailsAsync(string orderNumber, string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);
            if (client == null) return null;

            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null || order.ClientId != client.Id) return null;

            var orderItems = await _orderItemRepository.GetByOrderIdWithDetailsAsync(order.Id);
            var address = order.DeliveryAddressId.HasValue
                ? await _addressRepository.GetByIdAsync(order.DeliveryAddressId.Value)
                : null;

            return new ClientOrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                DeliveryFee = order.DeliveryFee,
                Notes = order.Notes,
                CreateDate = order.CreateDate,
                DeliveryAddress = address != null ? _mapper.Map<AddressDto>(address) : null,
                Items = _mapper.Map<List<OrderItemDetailDto>>(orderItems)
            };
        }

        public async Task<ClientStatsDto?> GetClientStatsAsync(string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);
            if (client == null) return null;

            return await GetClientStatsByIdAsync(client.Id);
        }

        public async Task<IEnumerable<ClientFavoriteProductDto>> GetClientFavoriteProductsAsync(string phone)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(phone);
            var client = await _clientRepository.GetByPhoneAsync(formattedPhone);
            if (client == null) return new List<ClientFavoriteProductDto>();

            var orders = await _orderRepository.GetByClientIdAsync(client.Id);
            var orderItems = new List<OrderItem>();

            foreach (var order in orders.Where(o => o.Status == OrderStatus.Confirmed))
            {
                var items = await _orderItemRepository.GetByOrderIdAsync(order.Id);
                orderItems.AddRange(items);
            }

            var productGroups = orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ClientFavoriteProductDto
                {
                    ProductId = g.Key,
                    OrderCount = g.Count(),
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    LastOrderDate = orders.Where(o => o.Id == g.First().OrderId).Max(o => o.CreateDate)
                })
                .OrderByDescending(p => p.OrderCount)
                .Take(10);

            return productGroups;
        }

        private static string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
