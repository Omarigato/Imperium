using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _smtpClient = new SmtpClient(_configuration["Email:Host"])
            {
                Port = int.Parse(_configuration["Email:Port"] ?? "587"),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                ),
                EnableSsl = true
            };
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string code)
        {
            try
            {
                var subject = "Код подтверждения - Imperium";
                var body = $@"
                    <html>
                    <body>
                        <h2>Добро пожаловать в Imperium!</h2>
                        <p>Ваш код подтверждения: <strong>{code}</strong></p>
                        <p>Код действителен в течение 15 минут.</p>
                        <br>
                        <p>Если вы не регистрировались на нашем сайте, проигнорируйте это письмо.</p>
                    </body>
                    </html>";

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Verification code sent successfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(string email, string orderNumber)
        {
            try
            {
                var subject = $"Подтверждение заказа #{orderNumber} - Imperium";
                var body = $@"
                    <html>
                    <body>
                        <h2>Спасибо за ваш заказ!</h2>
                        <p>Ваш заказ <strong>#{orderNumber}</strong> успешно оформлен.</p>
                        <p>Мы свяжемся с вами в ближайшее время для уточнения деталей доставки.</p>
                        <br>
                        <p>С уважением,<br>Команда Imperium</p>
                    </body>
                    </html>";

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Order confirmation sent successfully to {Email} for order {OrderNumber}", email, orderNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation to {Email} for order {OrderNumber}", email, orderNumber);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetLink)
        {
            try
            {
                var subject = "Сброс пароля - Imperium";
                var body = $@"
                    <html>
                    <body>
                        <h2>Сброс пароля</h2>
                        <p>Вы запросили сброс пароля для вашей учетной записи.</p>
                        <p>Перейдите по ссылке для создания нового пароля:</p>
                        <p><a href='{resetLink}'>Сбросить пароль</a></p>
                        <p>Ссылка действительна в течение 1 часа.</p>
                        <br>
                        <p>Если вы не запрашивали сброс пароля, проигнорируйте это письмо.</p>
                    </body>
                    </html>";

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:Username"]!, "Imperium"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await _smtpClient.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
