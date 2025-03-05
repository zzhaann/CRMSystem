using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult ForgotPassword()
        {
            return View();
        }


        public IActionResult Login()
        {
            return View();
        }

        public ActionResult Signup()
        {
            return View();
        }
    }
}
