using Imperium.Core.Models;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Admin;
using Imperium.Service.DTOs.Auth;
using Imperium.Service.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers
{
    /// <summary>
    /// Контроллер авторизации только для администраторов и менеджеров
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Вход в систему для админов и менеджеров
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<User>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.AdminLoginAsync(loginDto);
                return Ok(new
                {
                    success = true,
                    message = "Авторизация успешна",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed admin login attempt for {Login}", loginDto.Login);
                return Unauthorized(new
                {
                    success = false,
                    message = "Неверный логин или пароль"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin login for {Login}", loginDto.Login);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Внутренняя ошибка сервера"
                });
            }
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            try
            {
                // В JWT токенах logout обычно обрабатывается на клиенте
                // Здесь можно добавить логику для добавления токена в blacklist

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Admin user {UserId} logged out", userId);

                return Ok(new
                {
                    success = true,
                    message = "Выход выполнен успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при выходе из системы"
                });
            }
        }

        // <summary>
        /// Получить информацию о текущем пользователе
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AdminUserInfoDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                    return Unauthorized(new { success = false, message = "Неверный токен" });

                var userInfo = await _authService.GetAdminUserInfoAsync(userGuid);
                if (userInfo == null)
                    return NotFound(new { success = false, message = "Пользователь не найден" });

                return Ok(new
                {
                    success = true,
                    data = userInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка получения информации о пользователе"
                });
            }
        }

        /// <summary>
        /// Проверить действительность токена
        /// </summary>
        [HttpGet("validate-token")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                return Ok(new
                {
                    success = true,
                    valid = true,
                    user = new
                    {
                        id = userId,
                        name = userName,
                        role = userRole
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Ok(new
                {
                    success = false,
                    valid = false
                });
            }
        }

        /// <summary>
        /// Сменить пароль (для авторизованных админов)
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                    return Unauthorized(new { success = false, message = "Неверный токен" });

                var success = await _authService.ChangeAdminPasswordAsync(userGuid, changePasswordDto);
                if (!success)
                    return BadRequest(new { success = false, message = "Неверный текущий пароль" });

                return Ok(new
                {
                    success = true,
                    message = "Пароль успешно изменен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при изменении пароля"
                });
            }
        }

        /// <summary>
        /// Создать нового админа/менеджера (только для главного админа)
        /// </summary>
        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateAdmin([FromBody] CreateAdminDto createAdminDto)
        {
            try
            {
                var result = await _authService.CreateAdminAsync(createAdminDto);
                return Ok(new
                {
                    success = true,
                    message = "Администратор создан успешно",
                    adminId = result.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при создании администратора"
                });
            }
        }

        /// <summary>
        /// Получить список всех админов (только для главного админа)
        /// </summary>
        [HttpGet("admins")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AdminUserInfoDto>>> GetAllAdmins()
        {
            try
            {
                var admins = await _authService.GetAllAdminsAsync();
                return Ok(new
                {
                    success = true,
                    data = admins
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admins list");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка получения списка администраторов"
                });
            }
        }

        /// <summary>
        /// Деактивировать админа (только для главного админа)
        /// </summary>
        [HttpPut("deactivate-admin/{adminId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateAdmin(Guid adminId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == adminId.ToString())
                    return BadRequest(new { success = false, message = "Нельзя деактивировать самого себя" });

                var success = await _authService.DeactivateAdminAsync(adminId);
                if (!success)
                    return NotFound(new { success = false, message = "Администратор не найден" });

                return Ok(new
                {
                    success = true,
                    message = "Администратор деактивирован"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating admin {AdminId}", adminId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при деактивации администратора"
                });
            }
        }
    }
}