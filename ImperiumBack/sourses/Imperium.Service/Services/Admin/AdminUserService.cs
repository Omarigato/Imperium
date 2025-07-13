using AutoMapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories.User;
using Imperium.Service.DTOs.Admin;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AdminAuthResponseDto?> LoginAsync(AdminLoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByLoginAsync(loginDto.Login);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    _logger.LogWarning("Failed login attempt for {Login}", loginDto.Login);
                    return null;
                }

                // Проверяем, что это админ или менеджер
                if (user.Role != UserRole.Admin && user.Role != UserRole.Manager)
                {
                    _logger.LogWarning("Non-admin user {Login} tried to access admin panel", loginDto.Login);
                    return null;
                }

                return new AdminAuthResponseDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Login = user.Login,
                    IsAdmin = user.Role == UserRole.Admin,
                    Token = GenerateJwtToken(user), // TODO: Implement JWT generation
                    ExpiresAt = DateTime.UtcNow.AddHours(8) // 8 часов сессия для админов
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login for {Login}", loginDto.Login);
                return null;
            }
        }

        public async Task<AdminUserInfoDto?> GetCurrentUserInfoAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                return new AdminUserInfoDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Login = user.Login,
                    IsAdmin = user.Role == UserRole.Admin,
                    IsActive = user.DeleteDate == null,
                    CreateDate = user.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info for {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                // Проверяем текущий пароль
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                {
                    return false;
                }

                // Устанавливаем новый пароль
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _userRepository.Update(user);

                _logger.LogInformation("Password changed for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<AdminUserInfoDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveUsersAsync();

                return users.Select(u => new AdminUserInfoDto
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Login = u.Login,
                    IsAdmin = u.Role == UserRole.Admin,
                    IsActive = u.DeleteDate == null,
                    CreateDate = u.CreateDate
                }).OrderByDescending(u => u.CreateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<AdminUserInfoDto>();
            }
        }

        public async Task<AdminUserInfoDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                return new AdminUserInfoDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Login = user.Login,
                    IsAdmin = user.Role == UserRole.Admin,
                    IsActive = user.DeleteDate == null,
                    CreateDate = user.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return null;
            }
        }

        public async Task<AdminUserInfoDto> CreateUserAsync(CreateAdminDto createDto, Guid authorId)
        {
            try
            {
                // Проверяем, не существует ли уже пользователь с таким логином
                if (await _userRepository.LoginExistsAsync(createDto.Login))
                {
                    throw new InvalidOperationException("Пользователь с таким логином уже существует");
                }

                if (!string.IsNullOrEmpty(createDto.Phone) && await _userRepository.PhoneExistsAsync(createDto.Phone))
                {
                    throw new InvalidOperationException("Пользователь с таким телефоном уже существует");
                }

                var user = new User
                {
                    FullName = createDto.Name,
                    Login = createDto.Login,
                    Password = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                    Phone = createDto.Phone,
                    Role = createDto.IsAdmin ? UserRole.Admin : UserRole.Manager,
                    AuthorId = authorId,
                    CreateDate = DateTime.UtcNow
                };

                await _userRepository.Insert(user);

                _logger.LogInformation("New user created: {Login} by {AuthorId}", createDto.Login, authorId);

                return new AdminUserInfoDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Login = user.Login,
                    IsAdmin = user.Role == UserRole.Admin,
                    IsActive = true,
                    CreateDate = user.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Login}", createDto.Login);
                throw;
            }
        }

        public async Task<AdminUserInfoDto?> UpdateUserAsync(Guid userId, UpdateAdminDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                // Проверяем уникальность логина, если он изменился
                if (user.Login != updateDto.Login && await _userRepository.LoginExistsAsync(updateDto.Login))
                {
                    throw new InvalidOperationException("Пользователь с таким логином уже существует");
                }

                // Проверяем уникальность телефона, если он изменился
                if (user.Phone != updateDto.Phone &&
                    !string.IsNullOrEmpty(updateDto.Phone) &&
                    await _userRepository.PhoneExistsAsync(updateDto.Phone))
                {
                    throw new InvalidOperationException("Пользователь с таким телефоном уже существует");
                }

                user.FullName = updateDto.Name;
                user.Login = updateDto.Login;
                user.Phone = updateDto.Phone;

                if (updateDto.IsAdmin.HasValue)
                {
                    user.Role = updateDto.IsAdmin.Value ? UserRole.Admin : UserRole.Manager;
                }

                await _userRepository.Update(user);

                _logger.LogInformation("User updated: {UserId}", userId);

                return new AdminUserInfoDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Login = user.Login,
                    IsAdmin = user.Role == UserRole.Admin,
                    IsActive = user.DeleteDate == null,
                    CreateDate = user.CreateDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                // Мягкое удаление
                user.DeleteDate = DateTime.UtcNow;
                await _userRepository.Update(user);

                _logger.LogInformation("User soft deleted: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(Guid userId, Guid adminId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                // Генерируем временный пароль
                var tempPassword = GenerateTemporaryPassword();
                user.Password = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                await _userRepository.Update(user);

                _logger.LogInformation("Password reset for user {UserId} by admin {AdminId}", userId, adminId);

                // TODO: Отправить пароль по email или показать админу
                // Сейчас просто логируем для отладки
                _logger.LogInformation("Temporary password for user {UserId}: {TempPassword}", userId, tempPassword);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid adminId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                var oldRole = user.Role;
                user.Role = newRole;

                await _userRepository.Update(user);

                _logger.LogInformation("User role changed from {OldRole} to {NewRole} for user {UserId} by admin {AdminId}",
                    oldRole, newRole, userId, adminId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role for user {UserId}", userId);
                return false;
            }
        }

        public async Task<AdminUserStatsDto> GetUserStatisticsAsync()
        {
            try
            {
                var allUsers = await _userRepository.GetAllAsync();
                var activeUsers = allUsers.Where(u => u.DeleteDate == null);

                return new AdminUserStatsDto
                {
                    TotalUsers = allUsers.Count(),
                    ActiveUsers = activeUsers.Count(),
                    AdminUsers = activeUsers.Count(u => u.Role == UserRole.Admin),
                    ManagerUsers = activeUsers.Count(u => u.Role == UserRole.Manager),
                    InactiveUsers = allUsers.Count(u => u.DeleteDate != null),
                    UsersCreatedThisMonth = allUsers.Count(u => u.CreateDate >= DateTime.UtcNow.AddDays(-30)),
                    LastCreatedUser = allUsers.OrderByDescending(u => u.CreateDate).FirstOrDefault()?.FullName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return new AdminUserStatsDto();
            }
        }

        public async Task<IEnumerable<UserActivityDto>> GetMostActiveUsersAsync(int count = 10)
        {
            try
            {
                // TODO: Implement based on actual activity logs
                // Для примера возвращаем пустой список
                return new List<UserActivityDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most active users");
                return new List<UserActivityDto>();
            }
        }

        private static string GenerateTemporaryPassword()
        {
            // Генерируем простой временный пароль
            var random = new Random();
            return $"Temp{random.Next(100000, 999999)}!";
        }

        private string GenerateJwtToken(User user)
        {
            // TODO: Implement JWT token generation
            // Для примера возвращаем простую строку
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{user.Login}:{DateTime.UtcNow}"));
        }
    }
}
