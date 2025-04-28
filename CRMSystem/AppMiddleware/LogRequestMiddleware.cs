using System.Diagnostics;
namespace CRMSystem.AppMiddleware
{
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogRequestMiddleware> _logger;
        private Stopwatch _timer;

        public LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            _timer = Stopwatch.StartNew();
            _logger.LogInformation("Запрос начат: {method} {path}",
                context.Request.Method, context.Request.Path);

            await _next(context);

            _timer.Stop();
            var elapsedMs = _timer.ElapsedMilliseconds;
            _logger.LogInformation("Запрос завершен: {method} {path} за {ms} мс",
                context.Request.Method, context.Request.Path, elapsedMs);
        }
    }

    public static class LogRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogRequest(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogRequestMiddleware>();
        }
    }
}
