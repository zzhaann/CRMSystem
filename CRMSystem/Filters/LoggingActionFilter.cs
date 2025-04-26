using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace CRMSystem.Filters
{
    public class LoggingActionFilter : IActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        private Stopwatch _stopwatch;
        public LoggingActionFilter (ILogger<LoggingActionFilter> logger)
        {
            _logger = logger;
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
           _stopwatch.Stop();
            var username = context.HttpContext.User.Identity.IsAuthenticated
                ? context.HttpContext.User.Identity.Name
                : "Anon";
            _logger.LogInformation("End: {Action}, User: {User}, Time: {ElapsedMilliseconds} мс",
                context.ActionDescriptor.DisplayName, username, _stopwatch.ElapsedMilliseconds);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
            var username = context.HttpContext.User.Identity.IsAuthenticated
                ? context.HttpContext.User.Identity.Name
                : "Anon";

            _logger.LogInformation("Start: {Action}, User: {User}", context.ActionDescriptor.DisplayName, username);
        }
    }
}
