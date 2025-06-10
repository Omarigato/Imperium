using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Auth;
using Imperium.Service.DTOs.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.CustomLog
{
    public class CustomLogService : ICustomLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomLogService> _logger;

        public CustomLogService(IUnitOfWork unitOfWork, ILogger<CustomLogService> logger)
        {
            _unitOfWork = unitOfWork;
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

                await _unitOfWork.Logs.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();

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
                var logs = await _unitOfWork.Logs.GetAllAsync();

                var query = logs.AsQueryable();

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(l => l.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.CreatedAt <= toDate.Value);
                }

                var result = query
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new LogDto
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
                    })
                    .ToList();

                return result;
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
                var logs = await _unitOfWork.Logs.FindAsync(l => l.UserId == userId);

                var result = logs
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new LogDto
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
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user logs for {UserId}", userId);
                return new List<LogDto>();
            }
        }
    }
}
