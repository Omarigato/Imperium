using Imperium.Core.Enums;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Verification;
using Imperium.Service.Services.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(
            IVerificationService verificationService,
            ILogger<VerificationController> logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }

        [HttpPost("send-email-code")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> SendEmailVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _verificationService.SendVerificationCodeAsync(userId, request.Contact, VerificationType.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = result,
                    Message = $"Код отправлен на ваш email: {request.Contact}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("send-phone-code")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> SendPhoneVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _verificationService.SendVerificationCodeAsync(userId, request.Contact, VerificationType.Phone);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = result,
                    Message = "Код отправлен в WhatsApp"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending phone verification code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("verify-email")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyEmailCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isVerified = await _verificationService.VerifyCodeAsync(userId, request.Contact, request.Code, VerificationType.Email);

                if (isVerified)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Email успешно подтвержден"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Неверный или истекший код"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при подтверждении кода"
                });
            }
        }

        [HttpPost("verify-phone")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyPhoneCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isVerified = await _verificationService.VerifyCodeAsync(userId, request.Contact, request.Code, VerificationType.Phone);

                if (isVerified)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Телефон успешно подтвержден"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Неверный или истекший код"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при подтверждении кода"
                });
            }
        }

        [HttpPost("resend-email-code")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> ResendEmailVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _verificationService.ResendVerificationCodeAsync(userId, request.Contact, VerificationType.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = result,
                    Message = "Код повторно отправлен на email"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email verification code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("resend-phone-code")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> ResendPhoneVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _verificationService.ResendVerificationCodeAsync(userId, request.Contact, VerificationType.Phone);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = result,
                    Message = "Код повторно отправлен в WhatsApp"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending phone verification code for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<VerificationStatusResponse>>> GetVerificationStatus()
        {
            try
            {
                var userId = GetCurrentUserId();
                var emailVerified = await _verificationService.IsContactVerifiedAsync(userId, VerificationType.Email);
                var phoneVerified = await _verificationService.IsContactVerifiedAsync(userId, VerificationType.Phone);

                return Ok(new ApiResponse<VerificationStatusResponse>
                {
                    Success = true,
                    Data = new VerificationStatusResponse
                    {
                        IsEmailVerified = emailVerified,
                        IsPhoneVerified = phoneVerified
                    },
                    Message = "Статус верификации получен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification status for user {UserId}", GetCurrentUserId());
                return BadRequest(new ApiResponse<VerificationStatusResponse>
                {
                    Success = false,
                    Message = "Ошибка при получении статуса"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }  
}