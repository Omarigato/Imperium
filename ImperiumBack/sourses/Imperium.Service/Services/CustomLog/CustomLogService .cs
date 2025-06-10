// Imperium.Service/Services/CustomLog/CustomLogService.cs
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Service.DTOs.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.CustomLog
{
    public class CustomLogService : ICustomLogService
    {
        private readonly ILogRepository _logRepository;
        private readonly ILogger<CustomLogService> _logger;

        public CustomLogService(ILogRepository logRepository, ILogger<CustomLogService> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        public async Task LogInfoAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync("Info", message, null, userId, requestPath, requestMethod, ipAddress, userAgent);
        }

        public async Task LogWarningAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync("Warning", message, null, userId, requestPath, requestMethod, ipAddress, userAgent);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync("Error", message, exception, userId, requestPath, requestMethod, ipAddress, userAgent);
        }

        public async Task LogDebugAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync("Debug", message, null, userId, requestPath, requestMethod, ipAddress, userAgent);
        }

        private async Task LogAsync(string level, string message, Exception? exception, Guid? userId, string? requestPath, string? requestMethod, string? ipAddress, string? userAgent)
        {
            try
            {
                var log = new Log
                {
                    Level = level,
                    Message = message,
                    Exception = exception?.ToString(),
                    UserId = userId,
                    RequestPath = requestPath,
                    RequestMethod = requestMethod,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                await _logRepository.AddAsync(log);

                // Также логируем в стандартный ILogger
                switch (level.ToLower())
                {
                    case "info":
                        _logger.LogInformation("{Message} - User: {UserId}, Path: {RequestPath}", message, userId, requestPath);
                        break;
                    case "warning":
                        _logger.LogWarning("{Message} - User: {UserId}, Path: {RequestPath}", message, userId, requestPath);
                        break;
                    case "error":
                        _logger.LogError(exception, "{Message} - User: {UserId}, Path: {RequestPath}", message, userId, requestPath);
                        break;
                    case "debug":
                        _logger.LogDebug("{Message} - User: {UserId}, Path: {RequestPath}", message, userId, requestPath);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Если не можем записать в БД, логируем в стандартный logger
                _logger.LogError(ex, "Failed to save log to database: {Message}", message);
            }
        }

        public async Task<IEnumerable<LogDto>> GetLogsAsync(int page = 1, int pageSize = 50, string? level = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                IEnumerable<Log> logs;

                if (!string.IsNullOrEmpty(level))
                {
                    logs = await _logRepository.GetByLevelAsync(level, page, pageSize);
                }
                else if (fromDate.HasValue && toDate.HasValue)
                {
                    logs = await _logRepository.GetByDateRangeAsync(fromDate.Value, toDate.Value, page, pageSize);
                }
                else
                {
                    // Для общего случая берем последние записи с ограничением
                    var allLogs = await _logRepository.GetAllAsync();
                    var skip = (page - 1) * pageSize;
                    logs = allLogs.Skip(skip).Take(pageSize);
                }

                return logs.Select(l => new LogDto
                {
                    Id = l.Id,
                    Level = l.Level,
                    Message = l.Message,
                    Exception = l.Exception,
                    UserId = l.UserId,
                    RequestPath = l.RequestPath,
                    RequestMethod = l.RequestMethod,
                    IPAddress = l.IPAddress,
                    UserAgent = l.UserAgent,
                    CreatedAt = l.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs");
                return new List<LogDto>();
            }
        }

        public async Task<IEnumerable<LogDto>> GetUserLogsAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var logs = await _logRepository.GetByUserIdAsync(userId, page, pageSize);

                return logs.Select(l => new LogDto
                {
                    Id = l.Id,
                    Level = l.Level,
                    Message = l.Message,
                    Exception = l.Exception,
                    UserId = l.UserId,
                    RequestPath = l.RequestPath,
                    RequestMethod = l.RequestMethod,
                    IPAddress = l.IPAddress,
                    UserAgent = l.UserAgent,
                    CreatedAt = l.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user logs for {UserId}", userId);
                return new List<LogDto>();
            }
        }
    }
}