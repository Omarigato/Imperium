using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Imperium.Service.Services.WhatsApp
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _apiUrl;
        private readonly string _accessToken;
        private readonly string _adminPhone;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiUrl = _configuration["WhatsApp:ApiUrl"] ?? throw new ArgumentNullException("WhatsApp:ApiUrl");
            _accessToken = _configuration["WhatsApp:AccessToken"] ?? throw new ArgumentNullException("WhatsApp:AccessToken");
            _adminPhone = _configuration["WhatsApp:AdminPhone"] ?? throw new ArgumentNullException("WhatsApp:AdminPhone");

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            try
            {
                var message = $@"🔐 *Код подтверждения Imperium*

Ваш код: *{code}*

Код действителен 15 минут.

Если вы не регистрировались на нашем сайте, проигнорируйте это сообщение.";

                var success = await SendMessageAsync(phoneNumber, message);

                if (success)
                {
                    _logger.LogInformation("Verification code sent successfully to WhatsApp {PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogWarning("Failed to send verification code to WhatsApp {PhoneNumber}", phoneNumber);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code to WhatsApp {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendOrderNotificationAsync(string phoneNumber, string orderNumber, decimal totalAmount)
        {
            try
            {
                var message = $@"✅ *Заказ оформлен!*

Номер заказа: *{orderNumber}*
Сумма: *{totalAmount:N0} ₸*

Мы свяжемся с вами в ближайшее время для уточнения деталей доставки.

Спасибо за покупку в Imperium! 🛍️";

                var success = await SendMessageAsync(phoneNumber, message);

                if (success)
                {
                    _logger.LogInformation("Order notification sent successfully to WhatsApp {PhoneNumber} for order {OrderNumber}", phoneNumber, orderNumber);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order notification to WhatsApp {PhoneNumber} for order {OrderNumber}", phoneNumber, orderNumber);
                return false;
            }
        }

        public async Task<bool> NotifyAdminNewOrderAsync(string orderNumber, string customerName)
        {
            try
            {
                var message = $@"🔔 *Новый заказ!*

Заказ: *{orderNumber}*
Клиент: *{customerName}*

Проверьте детали в админ-панели.";

                var success = await SendMessageAsync(_adminPhone, message);

                if (success)
                {
                    _logger.LogInformation("Admin notification sent successfully for order {OrderNumber}", orderNumber);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notification for order {OrderNumber}", orderNumber);
                return false;
            }
        }

        private async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            try
            {
                // Форматируем номер телефона (убираем + и пробелы)
                var formattedPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = formattedPhone,
                    type = "text",
                    text = new { body = message }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("WhatsApp API response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("WhatsApp API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SendMessageAsync to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}
