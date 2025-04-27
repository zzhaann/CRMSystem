using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Admin.Controllers
{
    public class FlowersController : Controller
    {
        private readonly ILogger<FlowersController> _logger;
        private readonly AppDbContext _context;
        public FlowersController(ILogger<FlowersController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            var flowers = _context.Flowers.Include(f => f.Company).ToList();
            return View(flowers);
        }
        public IActionResult Edit(int id)
        {
            var fvm = new FlowerViewModel();
            fvm.Companies = _context.Companies.ToList();
            fvm.Flower = new Flower();
            if (id != 0)
            {
                var flower = _context.Flowers.Find(id);
                if (flower != null)
                {
                    fvm.Flower = flower;
                    return View(fvm);
                }
            }
            return View(fvm);
        }
        [HttpPost]
        public IActionResult Edit(Flower flower)
        {
            if (flower.Id != 0)
            {
                var _flower = _context.Flowers.Find(flower.Id);
                if (_flower != null)
                {
                    _flower.Name = flower.Name;
                    _flower.Price = flower.Price;
                    _flower.Quantity = flower.Quantity;
                    _flower.InitialQuantity = flower.InitialQuantity;
                    _flower.ClientPrice = flower.ClientPrice;
                    _flower.CompanyId = flower.CompanyId;
                    _context.SaveChanges();
                }
            }
            else
            {
                flower.CreatedAt = DateTime.Now;
                flower.CreatedBy = User.Identity.Name;
                _context.Flowers.Add(flower);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var flower = _context.Flowers.Find(id);
            if (flower != null)
            {
                _context.Flowers.Remove(flower);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}



