using Imperium.Service.Services.CustomLog;

namespace Imperium.Web
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                // Логируем в базу данных через сервис
                try
                {
                    using var scope = context.RequestServices.CreateScope();
                    var logService = scope.ServiceProvider.GetRequiredService<ICustomLogService>();

                    var userId = context.User?.FindFirst("sub")?.Value;
                    var userIdGuid = userId != null ? Guid.Parse(userId) : (Guid?)null;

                    await logService.LogErrorAsync(
                        $"Unhandled exception: {ex.Message}",
                        ex,
                        userIdGuid,
                        context.Request.Path,
                        context.Request.Method,
                        context.Connection.RemoteIpAddress?.ToString(),
                        context.Request.Headers["User-Agent"]
                    );
                }
                catch
                {
                    // Игнорируем ошибки логирования
                }

                throw;
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;

                if (duration.TotalMilliseconds > 1000) // Логируем медленные запросы
                {
                    _logger.LogWarning("Slow request: {Method} {Path} took {Duration}ms",
                        context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
                }
            }
        }
    }
}
