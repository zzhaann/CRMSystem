using CRMSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRMSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger, TokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            _logger.LogInformation("Login attempt for email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
                if (result.Succeeded)
                {
                    var accessToken = await _tokenService.GenerateAccessToken(user);
                    var refreshToken = _tokenService.GenerateRefreshToken();

                    // Сохраняем refresh token в базе данных
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                        Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"]));
                    await _userManager.UpdateAsync(user);

                    // Возвращаем токены
                    Response.Cookies.Append("jwtToken", accessToken);
                    Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true // в продакшене установите true для HTTPS
                    });

                    _logger.LogInformation("User {Email} logged in successfully.", email);
                    return RedirectToAction("Index", "Home");
                }
            }

            _logger.LogWarning("Invalid login attempt for email: {Email}", email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string username, string password)
        {
            _logger.LogInformation("Signup attempt for email: {Email}", email);

            var user = new ApplicationUser { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User {Email} signed up successfully.", email);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error during signup for email {Email}: {Error}", email, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Получаем текущего пользователя перед выходом
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Очищаем refresh token в базе данных
                    user.RefreshToken = "";
                    user.RefreshTokenExpiryTime = DateTime.MinValue;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("RefreshToken cleared for user ID: {UserId}", userId);
                }
            }

            // Удаляем куки с токенами
            Response.Cookies.Delete("jwtToken");
            Response.Cookies.Delete("refreshToken");
            _logger.LogInformation("JWT tokens removed from cookies");

            // Стандартный выход из системы через Identity
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            string accessToken = model.AccessToken;
            string refreshToken = model.RefreshToken;

            // Получаем информацию из токена
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Находим пользователя
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }

            // Обновляем токены
            var tokens = await _tokenService.RefreshTokens(accessToken, refreshToken, user);

            // Сохраняем обновленный RefreshToken в базу данных
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"]));
            await _userManager.UpdateAsync(user);

            // Возвращаем новые токены
            return Ok(new
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            });
        }

    }
}
