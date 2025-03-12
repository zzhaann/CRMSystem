using CRMSystem.Migrations;
using CRMSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRMSystem.Controllers
{
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserController> _logger;
        private const string DefaultProfilePhotoUrl = "/images/default-avatar.png";

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<UserController> logger) // Modify constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileModel
            {
                Name = user.UserName,
                Email = user.Email,
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change password model state is invalid.");
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}.", user.Id);
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error changing password for user {UserId}: {Error}", user.Id, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string currentPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return RedirectToAction("Login", "Account");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordCheck)
            {
                _logger.LogWarning("Incorrect password for user {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return View("DeleteAccount");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} deleted successfully.", user.Id);
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error deleting user {UserId}: {Error}", user.Id, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Profile");
        }
    }
}
