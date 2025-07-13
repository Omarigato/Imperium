using Imperium.Service.DTOs.Address;
using Imperium.Service.DTOs.Client;
using Imperium.Service.DTOs.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Client
{
    public interface IClientService
    {
        // Регистрация и авторизация
        Task<ClientAuthResponseDto> RegisterOrLoginAsync(ClientRegisterDto registerDto);
        Task<ClientAuthResponseDto> VerifyOtpAsync(ClientVerifyOtpDto verifyDto);
        Task<ClientAuthResponseDto> ResendOtpAsync(string phone);

        // Профиль клиента
        Task<ClientInfoDto?> GetByIdAsync(Guid clientId);
        Task<ClientInfoDto?> GetByPhoneAsync(string phone);
        Task<ClientInfoDto?> UpdateClientAsync(Guid clientId, UpdateClientDto updateDto);
        Task<ClientInfoDto?> UpdateClientContactAsync(UpdateContactDto updateContactDto);
        Task<bool> PhoneExistsAsync(string phone);

        // Адреса клиента
        Task<IEnumerable<AddressDto>> GetClientAddressesAsync(Guid clientId);
        Task<AddressDto> CreateAddressAsync(Guid clientId, CreateAddressDto createDto);
        Task<AddressDto?> UpdateAddressAsync(Guid clientId, Guid addressId, CreateAddressDto updateDto);
        Task<bool> DeleteAddressAsync(Guid clientId, Guid addressId);
        Task<bool> SetDefaultAddressAsync(Guid clientId, Guid addressId);

        // История и статистика
        Task<IEnumerable<ClientOrderHistoryDto>> GetClientOrdersHistoryAsync(string phone);
        Task<ClientOrderDetailsDto?> GetClientOrderDetailsAsync(string orderNumber, string phone);
        Task<ClientStatsDto?> GetClientStatsAsync(string phone);
        Task<IEnumerable<ClientFavoriteProductDto>> GetClientFavoriteProductsAsync(string phone);
    }
}
