using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRMSystem.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter

    {

        private readonly ILogger<GlobalExceptionFilter> _logger;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }



        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Необработанная ошибка в приложении");
            context.Result = new RedirectToActionResult(
                "Exception",
                "Home",
                 new { message = context.Exception.Message }
                );
            context.ExceptionHandled = true;
        }
    }
}
