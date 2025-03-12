using CRMSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CRMSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<UserController> logger) // Modify constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            _logger.LogInformation("Accessing profile page.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change password model state is invalid.");
                return View(model);
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
