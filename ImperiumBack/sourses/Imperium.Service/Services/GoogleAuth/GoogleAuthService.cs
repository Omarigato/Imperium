using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Imperium.Service.Services.GoogleAuth
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly string _clientId;

        public GoogleAuthService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _clientId = _configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException("GoogleAuth:ClientId");
        }

        public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google token verification failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenInfo == null || tokenInfo.Aud != _clientId)
                {
                    _logger.LogWarning("Invalid Google token or client ID mismatch");
                    return null;
                }

                return new GoogleUserInfo
                {
                    Id = tokenInfo.Sub,
                    Email = tokenInfo.Email,
                    Name = tokenInfo.Name,
                    Picture = tokenInfo.Picture,
                    EmailVerified = tokenInfo.EmailVerified == "true"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google token");
                return null;
            }
        }

        private class GoogleTokenResponse
        {
            public string Sub { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Picture { get; set; } = string.Empty;
            public string Aud { get; set; } = string.Empty;
            public string EmailVerified { get; set; } = string.Empty;
        }
    }
}
