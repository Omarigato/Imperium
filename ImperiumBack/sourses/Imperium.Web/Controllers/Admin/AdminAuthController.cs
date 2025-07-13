using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Admin;
using Imperium.Service.DTOs.Review;
using Imperium.Service.Services.Admin;
using Imperium.Service.Services.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для авторизации админов
    /// </summary>
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ILogger<AdminAuthController> _logger;

        public AdminAuthController(IAdminUserService adminUserService, ILogger<AdminAuthController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }

        /// <summary>
        /// Авторизация админа
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AdminAuthResponseDto>>> Login([FromBody] AdminLoginDto loginDto)
        {
            try
            {
                var result = await _adminUserService.LoginAsync(loginDto);

                if (result == null)
                {
                    return Unauthorized(new ApiResponse<AdminAuthResponseDto>
                    {
                        Success = false,
                        Message = "Неверный логин или пароль"
                    });
                }

                return Ok(new ApiResponse<AdminAuthResponseDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Авторизация успешна"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login");
                return StatusCode(500, new ApiResponse<AdminAuthResponseDto>
                {
                    Success = false,
                    Message = "Ошибка при авторизации"
                });
            }
        }

        /// <summary>
        /// Получить информацию о текущем админе
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<AdminUserInfoDto>>> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userInfo = await _adminUserService.GetCurrentUserInfoAsync(userId);

                if (userInfo == null)
                {
                    return NotFound(new ApiResponse<AdminUserInfoDto>
                    {
                        Success = false,
                        Message = "Пользователь не найден"
                    });
                }

                return Ok(new ApiResponse<AdminUserInfoDto>
                {
                    Success = true,
                    Data = userInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, new ApiResponse<AdminUserInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при получении информации о пользователе"
                });
            }
        }

        /// <summary>
        /// Смена пароля
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _adminUserService.ChangePasswordAsync(userId, changePasswordDto);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Пароль изменен успешно" : "Неверный текущий пароль"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при смене пароля"
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
