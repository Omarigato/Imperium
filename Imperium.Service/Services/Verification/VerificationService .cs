using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.Services.Email;
using Imperium.Service.Services.WhatsApp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Verification
{
    public class VerificationService : IVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<VerificationService> _logger;
        private const int MaxAttempts = 5;
        private const int CodeExpiryMinutes = 15;

        public VerificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            ILogger<VerificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task<string> SendVerificationCodeAsync(Guid userId, string contact, VerificationType type)
        {
            try
            {
                // Проверяем существующие активные коды
                var existingVerifications = await _unitOfWork.Verifications.FindAsync(v =>
                    v.UserId == userId &&
                    v.Contact == contact &&
                    v.Type == type.ToString() &&
                    v.ExpiresAt > DateTime.UtcNow &&
                    !v.IsVerified);

                // Если есть активные коды, проверяем количество попыток
                var activeVerification = existingVerifications.FirstOrDefault();
                if (activeVerification != null && activeVerification.AttemptCount >= MaxAttempts)
                {
                    throw new InvalidOperationException("Превышено максимальное количество попыток. Попробуйте позже.");
                }

                // Генерируем новый код
                var code = GenerateVerificationCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(CodeExpiryMinutes);

                // Создаем новую запись верификации
                var verification = new Core.Models.Verification
                {
                    UserId = userId,
                    Type = type.ToString(),
                    Contact = contact,
                    Code = code,
                    ExpiresAt = expiresAt,
                    AttemptCount = 1
                };

                await _unitOfWork.Verifications.AddAsync(verification);
                await _unitOfWork.SaveChangesAsync();

                // Отправляем код
                bool sent = false;
                if (type == VerificationType.Email)
                {
                    sent = await _emailService.SendVerificationCodeAsync(contact, code);
                }
                else if (type == VerificationType.Phone)
                {
                    sent = await _whatsAppService.SendVerificationCodeAsync(contact, code);
                }

                if (!sent)
                {
                    throw new InvalidOperationException("Не удалось отправить код верификации. Попробуйте позже.");
                }

                _logger.LogInformation("Verification code sent to {Contact} of type {Type} for user {UserId}",
                    contact, type, userId);

                return "Код верификации отправлен";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code to {Contact} of type {Type} for user {UserId}",
                    contact, type, userId);
                throw;
            }
        }

        public async Task<bool> VerifyCodeAsync(Guid userId, string contact, string code, VerificationType type)
        {
            try
            {
                var verification = await _unitOfWork.Verifications.GetSingleOrDefaultAsync(v =>
                    v.UserId == userId &&
                    v.Contact == contact &&
                    v.Type == type.ToString() &&
                    v.Code == code &&
                    v.ExpiresAt > DateTime.UtcNow &&
                    !v.IsVerified);

                if (verification == null)
                {
                    _logger.LogWarning("Invalid verification attempt for {Contact} of type {Type} for user {UserId}",
                        contact, type, userId);
                    return false;
                }

                // Помечаем как верифицированный
                verification.IsVerified = true;
                _unitOfWork.Verifications.Update(verification);

                // Обновляем статус верификации пользователя
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    if (type == VerificationType.Email)
                    {
                        user.IsEmailVerified = true;
                    }
                    else if (type == VerificationType.Phone)
                    {
                        user.IsPhoneVerified = true;
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Users.Update(user);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully verified {Contact} of type {Type} for user {UserId}",
                    contact, type, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying code for {Contact} of type {Type} for user {UserId}",
                    contact, type, userId);
                return false;
            }
        }

        public async Task<string> ResendVerificationCodeAsync(Guid userId, string contact, VerificationType type)
        {
            try
            {
                // Проверяем последний отправленный код
                var lastVerification = await _unitOfWork.Verifications
                    .FindAsync(v => v.UserId == userId && v.Contact == contact && v.Type == type.ToString());

                var recentVerification = lastVerification
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefault();

                // Проверяем, не слишком ли часто запрашивают повторную отправку
                if (recentVerification != null &&
                    recentVerification.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
                {
                    throw new InvalidOperationException("Подождите 1 минуту перед повторной отправкой кода.");
                }

                // Отправляем новый код
                return await SendVerificationCodeAsync(userId, contact, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code to {Contact} of type {Type} for user {UserId}",
                    contact, type, userId);
                throw;
            }
        }

        public async Task<bool> IsContactVerifiedAsync(Guid userId, VerificationType type)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return false;

                return type switch
                {
                    VerificationType.Email => user.IsEmailVerified,
                    VerificationType.Phone => user.IsPhoneVerified,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking verification status for user {UserId} and type {Type}",
                    userId, type);
                return false;
            }
        }

        private static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
