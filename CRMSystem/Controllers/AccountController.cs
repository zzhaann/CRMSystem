using CRMSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
                    // Генерируем access и refresh токены
                    var accessToken = await _tokenService.GenerateAccessToken(user);
                    var refreshToken = _tokenService.GenerateRefreshToken();

                    // Устанавливаем токены в куки
                    Response.Cookies.Append("jwtToken", accessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Strict
                    });

                    Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.Now.AddDays(
                            Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"))
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
        [Route("api/account/refreshtoken")]
        public IActionResult RefreshToken()
        {
            // Получаем токены из куки
            var accessToken = Request.Cookies["jwtToken"];
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("No token provided");
            }

            // Проверяем истек ли access token
            if (!_tokenService.IsTokenExpired(accessToken))
            {
                return BadRequest("Access token is still valid");
            }

            // Получаем информацию из токена
            var principal = _tokenService.GetPrincipalFromToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token");
            }

            try
            {
                // Генерируем новый access token
                var newAccessToken = _tokenService.GenerateAccessTokenFromPrincipal(principal);

                // Устанавливаем новый access token в куки
                Response.Cookies.Append("jwtToken", newAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict
                });

                return Ok(new { message = "Token refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return BadRequest("Error refreshing token");
            }
        }
    }
}
