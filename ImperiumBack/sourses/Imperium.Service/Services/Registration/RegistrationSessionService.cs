using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Registration
{
    public class RegistrationSessionService : IRegistrationSessionService
    {
        private readonly IMemoryCache _cache;
        private const int SESSION_EXPIRY_MINUTES = 30;

        public RegistrationSessionService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task StoreRegistrationDataAsync(string contact, string fullName, string type)
        {
            var key = GetCacheKey(contact);
            var data = new RegistrationData
            {
                FullName = fullName,
                Contact = contact,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _cache.Set(key, data, TimeSpan.FromMinutes(SESSION_EXPIRY_MINUTES));
            await Task.CompletedTask;
        }

        public async Task<RegistrationData?> GetRegistrationDataAsync(string contact)
        {
            var key = GetCacheKey(contact);
            _cache.TryGetValue(key, out RegistrationData? data);
            return await Task.FromResult(data);
        }

        public async Task ClearRegistrationDataAsync(string contact)
        {
            var key = GetCacheKey(contact);
            _cache.Remove(key);
            await Task.CompletedTask;
        }

        private static string GetCacheKey(string contact)
        {
            return $"registration_session_{contact}";
        }
    }
}
