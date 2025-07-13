using Imperium.Core.Enums;
using Imperium.Service.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Admin
{
    public interface IAdminUserService
    {
        // Авторизация админов
        Task<AdminAuthResponseDto?> LoginAsync(AdminLoginDto loginDto);
        Task<AdminUserInfoDto?> GetCurrentUserInfoAsync(Guid userId);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        // Управление пользователями
        Task<IEnumerable<AdminUserInfoDto>> GetAllUsersAsync();
        Task<AdminUserInfoDto?> GetUserByIdAsync(Guid userId);
        Task<AdminUserInfoDto> CreateUserAsync(CreateAdminDto createDto, Guid authorId);
        Task<AdminUserInfoDto?> UpdateUserAsync(Guid userId, UpdateAdminDto updateDto);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> ResetUserPasswordAsync(Guid userId, Guid adminId);
        Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid adminId);

        // Статистика пользователей
        Task<AdminUserStatsDto> GetUserStatisticsAsync();
        Task<IEnumerable<UserActivityDto>> GetMostActiveUsersAsync(int count = 10);
    }
}
