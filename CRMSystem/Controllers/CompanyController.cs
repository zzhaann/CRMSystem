using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using CRMSystem.Filters;

namespace CRMSystem.Controllers
{

    [ServiceFilter(typeof(LoggingActionFilter))]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var companies = await _context.Companies
                .Include(c => c.Flowers)
                .ToListAsync();

            ViewBag.Companies = companies;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CompanyWithFlowerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";

                var companies = await _context.Companies
                    .Include(c => c.Flowers)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(model);
            }

            var company = new Company
            {
                Name = model.Name,
                Address = model.Address,
                ContactPhone = model.ContactPhone
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var flower = new Flower
            {
                Name = model.FlowerName,
                Quantity = model.Quantity,
                Price = model.Price,
                CompanyId = company.Id,
                ClientPrice = model.ClientPrice,
                InitialQuantity = model.Quantity
            };

            _context.Flowers.Add(flower);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Компания и цветы успешно добавлены!";
            return RedirectToAction("Add");
        }
    }
}
