using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.Controllers
{
    public class InboxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
