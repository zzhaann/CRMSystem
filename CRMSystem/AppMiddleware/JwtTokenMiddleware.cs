using CRMSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CRMSystem.AppMiddleware
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public JwtTokenMiddleware(
            RequestDelegate next,
            ILogger<JwtTokenMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Invoke(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService)
        {
            if (ShouldSkipMiddleware(context))
            {
                await _next(context);
                return;
            }

            var accessToken = context.Request.Cookies["jwtToken"];
            var refreshToken = context.Request.Cookies["refreshToken"];

            // Если токенов нет, перенаправляем на логин
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogInformation("Токены отсутствуют, перенаправление на страницу входа");
                context.Response.Redirect("/Account/Login");
                return;
            }

            try
            {
                // Проверяем, истек ли access token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(accessToken);
                var tokenExpiryTime = jwtToken.ValidTo;

                if (tokenExpiryTime < DateTime.UtcNow)
                {
                    _logger.LogInformation("Access token истек, пробуем обновить через refresh token");

                    var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
                    var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogWarning("Невозможно извлечь ID пользователя из токена");
                        await LogoutAndRedirect(context);
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        _logger.LogWarning("Пользователь не найден");
                        await LogoutAndRedirect(context);
                        return;
                    }

                    // Проверяем валидность refresh token
                    if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                    {
                        _logger.LogWarning("Refresh token недействителен или истек");
                        await LogoutAndRedirect(context);
                        return;
                    }

                    // Генерируем новые токены
                    var tokens = await tokenService.RefreshTokens(accessToken, refreshToken, user);

                    // Обновляем refresh token в базе данных
                    user.RefreshToken = tokens.RefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                        Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
                    await userManager.UpdateAsync(user);

                    // Устанавливаем новые токены в куки
                    context.Response.Cookies.Append("jwtToken", tokens.AccessToken);
                    context.Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true
                    });

                    _logger.LogInformation("Токены успешно обновлены");
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Ошибка при проверке токена");
                await LogoutAndRedirect(context);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при проверке токенов");
                await LogoutAndRedirect(context);
                return;
            }

            await _next(context);
        }

        private bool ShouldSkipMiddleware(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            return path != null && (
                path.Equals("/") ||
                path.StartsWith("/account/login") ||
                path.StartsWith("/account/signup") ||
                path.StartsWith("/account/forgotpassword") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/images/") ||
                path.EndsWith(".ico")
            );
        }

        private async Task LogoutAndRedirect(HttpContext context)
        {
            context.Response.Cookies.Delete("jwtToken");
            context.Response.Cookies.Delete("refreshToken");

            await context.SignOutAsync(IdentityConstants.ApplicationScheme);

            context.Response.Redirect("/Account/Login");
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
