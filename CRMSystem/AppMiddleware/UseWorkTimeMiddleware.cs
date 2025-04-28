namespace CRMSystem.AppMiddleware
{
    public class UseWorkTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public UseWorkTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var hour = DateTime.Now.Hour;

            
            if (hour < 7 || hour >= 18)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("CRM работает только с 9:00 до 18:00. Попробуйте позже.");
                return;
            }

            await _next(context);
        }
    }

    public static class UseWorkTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseWorkTime(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UseWorkTimeMiddleware>();
        }
    }
}
