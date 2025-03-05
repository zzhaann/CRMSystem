using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
