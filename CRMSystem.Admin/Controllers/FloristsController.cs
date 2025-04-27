using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.Admin.Controllers
{
    public class FloristsController : Controller
    {
        private readonly ILogger<FloristsController> _logger;
        private readonly AppDbContext _context;

        public FloristsController(ILogger<FloristsController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            var florists = _context.Florists.ToList();
            return View(florists);
        }
        public IActionResult Edit(int id)
        {
            if (id != 0)
            {
                var florist = _context.Florists.Find(id);
                return View(florist);
            }
            return View(new Florist());
        }
        [HttpPost]
        public IActionResult Edit(Florist florist)
        {
            if (florist.Id != 0)
            {
                var _florist = _context.Florists.Find(florist.Id);
                if (_florist != null)
                {
                    _florist.FullName = florist.FullName;
                    _florist.Phone = florist.Phone;
                }
            }
            else
            {
                florist.CreatedAt = DateTime.Now;
                florist.CreatedBy = User.Identity.Name;
                _context.Florists.Add(florist);
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var florist = _context.Florists.Find(id);
            if (florist != null)
            {
                _context.Florists.Remove(florist);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
