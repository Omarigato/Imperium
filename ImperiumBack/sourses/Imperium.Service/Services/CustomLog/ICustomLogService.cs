using Imperium.Core.Models;
using Imperium.Service.DTOs.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Service.Services.CustomLog
{
    public interface ICustomLogService
    {
        Task LogInfoAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null);
        Task LogWarningAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null);
        Task LogErrorAsync(string message, Exception? exception = null, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null);
        Task LogDebugAsync(string message, Guid? userId = null, string? requestPath = null, string? requestMethod = null, string? ipAddress = null, string? userAgent = null);
        Task<IEnumerable<LogDto>> GetLogsAsync(int page = 1, int pageSize = 50, string? level = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<LogDto>> GetUserLogsAsync(Guid userId, int page = 1, int pageSize = 50);
    }
}
