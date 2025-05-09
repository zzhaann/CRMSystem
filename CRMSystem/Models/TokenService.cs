using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRMSystem.Models
{
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            //полезная нагрузка
            var claim = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            //заголовок
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //"AccessTokenExpirationMinutes": 15
            var token = new JwtSecurityToken(

                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claim,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"])),

                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        // Метод генерации Refresh Token
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        // Метод для проверки и обновления токенов
        public async Task<(string AccessToken, string RefreshToken)> RefreshTokens(string expiredAccessToken, string refreshToken, ApplicationUser user)
        {
            // Проверяем валидность Refresh Token (должен быть реализован в вашей модели пользователя)
            // Этот метод предполагает, что у вас есть поле RefreshToken и RefreshTokenExpiryTime в ApplicationUser
            if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Генерируем новые токены
            string newAccessToken = await GenerateAccessToken(user);
            string newRefreshToken = GenerateRefreshToken();

            // Обновляем Refresh Token в базе данных
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));

            return (newAccessToken, newRefreshToken);
        }
        // Метод для проверки валидности токена
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false // Не проверяем срок действия, так как токен может быть истекшим
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            // Проверяем, что токен использует правильный алгоритм безопасности
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
