using Imperium.Core.Models;
using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Admin;
using Imperium.Service.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Imperium.Web.Controllers.Admin
{
    /// <summary>
    /// Контроллер для управления пользователями (только админы)
    /// </summary>
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IAdminUserService adminUserService, ILogger<AdminUsersController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdminUserInfoDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _adminUserService.GetAllUsersAsync();

                return Ok(new ApiResponse<IEnumerable<AdminUserInfoDto>>
                {
                    Success = true,
                    Data = users,
                    Message = $"Найдено {users.Count()} пользователей"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new ApiResponse<IEnumerable<AdminUserInfoDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении пользователей"
                });
            }
        }

        /// <summary>
        /// Создать пользователя
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AdminUserInfoDto>>> CreateUser([FromBody] CreateAdminDto createDto)
        {
            try
            {
                var authorId = GetCurrentUserId();
                var user = await _adminUserService.CreateUserAsync(createDto, authorId);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new ApiResponse<AdminUserInfoDto>
                {
                    Success = true,
                    Data = user,
                    Message = "Пользователь создан успешно"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<AdminUserInfoDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ApiResponse<AdminUserInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при создании пользователя"
                });
            }
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdminUserInfoDto>>> GetUserById(Guid id)
        {
            try
            {
                var user = await _adminUserService.GetUserByIdAsync(id);

                if (user == null)
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
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new ApiResponse<AdminUserInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при получении пользователя"
                });
            }
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
        {
            try
            {
                var success = await _adminUserService.DeleteUserAsync(id);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Пользователь удален" : "Пользователь не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при удалении пользователя"
                });
            }
        }

        /// <summary>
        /// Сбросить пароль пользователя
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetUserPassword(Guid id)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var success = await _adminUserService.ResetUserPasswordAsync(id, adminId);

                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Data = success,
                    Message = success ? "Пароль сброшен" : "Пользователь не найден"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ошибка при сбросе пароля"
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
