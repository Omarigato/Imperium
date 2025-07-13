
using Imperium.Service.DTOs.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramService> _logger;
        private readonly string _botToken;
        private readonly string _chatId;
        private readonly string _apiUrl;

        public TelegramService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TelegramService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _botToken = _configuration["Telegram:BotToken"] ?? throw new ArgumentNullException("Telegram:BotToken not configured");
            _chatId = _configuration["Telegram:ChatId"] ?? throw new ArgumentNullException("Telegram:ChatId not configured");
            _apiUrl = $"https://api.telegram.org/bot{_botToken}";
        }

        public async Task<bool> SendNewOrderNotificationAsync(string orderNumber, string clientName, string clientPhone, decimal totalAmount, string orderUrl)
        {
            try
            {
                var message = $@"🛒 <b>НОВЫЙ ЗАКАЗ!</b>

📝 <b>Номер заказа:</b> {orderNumber}
👤 <b>Клиент:</b> {clientName}
📞 <b>Телефон:</b> {clientPhone}
💰 <b>Сумма:</b> {totalAmount:N0} ₸

🔗 <a href=""{orderUrl}"">Открыть заказ в админ-панели</a>

⚡️ Нажмите на ссылку, чтобы взять заказ в работу";

                return await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new order notification for order {OrderNumber}", orderNumber);
                return false;
            }
        }

        public async Task<bool> SendOrderStatusUpdateAsync(string orderNumber, string newStatus, string managerName)
        {
            try
            {
                var statusEmoji = newStatus switch
                {
                    "Pending" => "⏳",
                    "Confirmed" => "✅",
                    "Cancelled" => "❌",
                    _ => "📋"
                };

                var statusText = newStatus switch
                {
                    "Pending" => "Взят в работу",
                    "Confirmed" => "Подтвержден",
                    "Cancelled" => "Отменен",
                    _ => newStatus
                };

                var message = $@"{statusEmoji} <b>СТАТУС ЗАКАЗА ИЗМЕНЕН</b>

📝 <b>Заказ:</b> {orderNumber}
📊 <b>Новый статус:</b> {statusText}
👨‍💼 <b>Менеджер:</b> {managerName}
🕐 <b>Время:</b> {DateTime.Now:dd.MM.yyyy HH:mm}";

                return await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order status update for order {OrderNumber}", orderNumber);
                return false;
            }
        }

        public async Task<bool> SendTestMessageAsync(string message)
        {
            try
            {
                var testMessage = $@"🧪 <b>ТЕСТОВОЕ СООБЩЕНИЕ</b>

{message}

🕐 {DateTime.Now:dd.MM.yyyy HH:mm:ss}";

                return await SendMessageAsync(testMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test message");
                return false;
            }
        }

        private async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                var payload = new
                {
                    chat_id = _chatId,
                    text = message,
                    parse_mode = "HTML",
                    disable_web_page_preview = false
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiUrl}/sendMessage", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Telegram message sent successfully: {Response}", responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Telegram API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SendMessageAsync to Telegram");
                return false;
            }
        }

        /// <summary>
        /// Проверяет подключение к Telegram Bot API
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/getMe");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TelegramApiResponse>(content);

                    if (result?.Ok == true)
                    {
                        _logger.LogInformation("Telegram bot connection successful. Bot name: {BotName}",
                            result.Result?.FirstName);
                        return true;
                    }
                }

                _logger.LogWarning("Telegram bot connection failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Telegram connection");
                return false;
            }
        }

        /// <summary>
        /// Получает информацию о чате
        /// </summary>
        public async Task<string?> GetChatInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/getChat?chat_id={_chatId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat info");
                return null;
            }
        }
    }
}
