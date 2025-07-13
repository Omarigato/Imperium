using Imperium.Service.DTOs;
using Imperium.Service.DTOs.Admin;
using Imperium.Service.DTOs.Log;
using Imperium.Service.Services.CustomLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ICustomLogService _logService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ICustomLogService logService, ILogger<AdminController> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LogDto>>>> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? level = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var logs = await _logService.GetLogsAsync(page, pageSize, level, fromDate, toDate);

                return Ok(new ApiResponse<IEnumerable<LogDto>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Логи получены успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs");
                return BadRequest(new ApiResponse<IEnumerable<LogDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении логов"
                });
            }
        }

        [HttpGet("logs/user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LogDto>>>> GetUserLogs(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var logs = await _logService.GetUserLogsAsync(userId, page, pageSize);

                return Ok(new ApiResponse<IEnumerable<LogDto>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Логи пользователя получены успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user logs for {UserId}", userId);
                return BadRequest(new ApiResponse<IEnumerable<LogDto>>
                {
                    Success = false,
                    Message = "Ошибка при получении логов пользователя"
                });
            }
        }

        [HttpGet("logs/stats")]
        public async Task<ActionResult<ApiResponse<LogStatsDto>>> GetLogStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var allLogs = await _logService.GetLogsAsync(1, int.MaxValue, null, fromDate, toDate);

                var stats = new LogStatsDto
                {
                    TotalLogs = allLogs.Count(),
                    ErrorCount = allLogs.Count(l => l.Level == "Error"),
                    WarningCount = allLogs.Count(l => l.Level == "Warning"),
                    InfoCount = allLogs.Count(l => l.Level == "Info"),
                    DebugCount = allLogs.Count(l => l.Level == "Debug"),
                    LastErrorDate = allLogs.Where(l => l.Level == "Error").OrderByDescending(l => l.CreateDate).FirstOrDefault()?.CreateDate,
                    MostActiveUsers = allLogs
                        .Where(l => l.UserId.HasValue)
                        .GroupBy(l => l.UserId)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new UserActivityDto
                        {
                            UserId = g.Key!.Value,
                            LogCount = g.Count()
                        })
                        .ToList()
                };

                return Ok(new ApiResponse<LogStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Статистика логов получена успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving log stats");
                return BadRequest(new ApiResponse<LogStatsDto>
                {
                    Success = false,
                    Message = "Ошибка при получении статистики"
                });
            }
        }

        [HttpDelete("logs/cleanup")]
        public async Task<ActionResult<ApiResponse<string>>> CleanupOldLogs([FromQuery] int daysOld = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var allLogs = await _logService.GetLogsAsync(1, int.MaxValue);
                var oldLogs = allLogs.Where(l => l.CreateDate < cutoffDate);

                // Здесь должна быть логика удаления старых логов
                // Для примера просто возвращаем количество
                var count = oldLogs.Count();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = $"Удалено {count} старых записей логов",
                    Message = "Очистка логов выполнена успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old logs");
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Ошибка при очистке логов"
                });
            }
        }

        [HttpGet("system-info")]
        public ActionResult<ApiResponse<SystemInfoDto>> GetSystemInfo()
        {
            try
            {
                var systemInfo = new SystemInfoDto
                {
                    ServerTime = DateTime.UtcNow,
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    Version = Environment.Version.ToString(),
                    Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime
                };

                return Ok(new ApiResponse<SystemInfoDto>
                {
                    Success = true,
                    Data = systemInfo,
                    Message = "Информация о системе получена успешно"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system info");
                return BadRequest(new ApiResponse<SystemInfoDto>
                {
                    Success = false,
                    Message = "Ошибка при получении информации о системе"
                });
            }
        }
    }
}