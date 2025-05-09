using CRMSystem.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CRMSystem.AppMiddleware
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenMiddleware> _logger;

        public JwtTokenMiddleware(
            RequestDelegate next,
            ILogger<JwtTokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, TokenService tokenService)
        {
            // Извлекаем токены из куки для любого запроса
            var accessToken = context.Request.Cookies["jwtToken"];
            var refreshToken = context.Request.Cookies["refreshToken"];

            // Если токенов нет, пропускаем middleware
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                await _next(context);
                return;
            }

            // Проверяем истек ли access token
            if (tokenService.IsTokenExpired(accessToken))
            {
                _logger.LogInformation("Access token истек, обновляем с помощью refresh token");

                // Получаем информацию из access token
                var principal = tokenService.GetPrincipalFromToken(accessToken);
                if (principal == null)
                {
                    _logger.LogWarning("Не удалось извлечь данные из access token");
                    await _next(context);
                    return;
                }

                try
                {
                    // Генерируем новый access token
                    var newAccessToken = tokenService.GenerateAccessTokenFromPrincipal(principal);

                    // Устанавливаем новый access token в куки
                    context.Response.Cookies.Append("jwtToken", newAccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Lax // Изменено с Strict для работы с внешними API
                    });

                    _logger.LogInformation("Access token успешно обновлен");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении access token");
                }
            }

            await _next(context);
        }
    }

    public static class JwtTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtTokenMiddleware>();
        }
    }
}
