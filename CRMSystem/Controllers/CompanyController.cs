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
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Clients()
        {
            var clients = await _context.Clients.ToListAsync();

            return View(clients);
        }
        [HttpPost]
        public async Task<IActionResult> AddClient(Client client)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Clients");
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Клиент успешно добавлен!";
            return RedirectToAction("Clients");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Клиент не найден!";
                return RedirectToAction("Clients");
            }

            if (client.Orders.Any())
            {
                TempData["ErrorMessage"] = "Невозможно удалить клиента с существующими заказами!";
                return RedirectToAction("Clients");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Клиент успешно удален!";
            return RedirectToAction("Clients");
        }
        public async Task<IActionResult> Florists()
        {
            var florists = await _context.Florists.ToListAsync();
            return View(florists);
        }
        [HttpPost]
        public async Task<IActionResult> AddFlorist(Florist florist)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Florists");
            }

            _context.Florists.Add(florist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Флорист успешно добавлен!";
            return RedirectToAction("Florists");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteFlorist(int id)
        {
            var florist = await _context.Florists.FirstOrDefaultAsync(f => f.Id == id);

            if (florist == null)
            {
                TempData["ErrorMessage"] = "Флорист не найден!";
                return RedirectToAction("Florists");
            }

            _context.Florists.Remove(florist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Флорист успешно удалён!";
            return RedirectToAction("Florists");
        }

        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies
                .Include(c => c.Flowers)
                .ToListAsync();
            return View(companies);
        }
        [HttpPost]
        public async Task<IActionResult> AddCompany(Company company)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Companies");
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Компания успешно добавлена!";
            return RedirectToAction("Companies");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Flowers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                TempData["ErrorMessage"] = "Компания не найдена!";
                return RedirectToAction("Companies");
            }

            if (company.Flowers.Any())
            {
                TempData["ErrorMessage"] = "Невозможно удалить компанию с существующими цветами!";
                return RedirectToAction("Companies");
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Компания успешно удалена!";
            return RedirectToAction("Companies");
        }
        public async Task<IActionResult> Flowers()
        {
            var flowers = await _context.Flowers
                .Include(f => f.Company)
                .ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(flowers);
        }

        [HttpPost]
        public async Task<IActionResult> AddFlower(Flower flower)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Flowers");
            }

            _context.Flowers.Add(flower);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Цветок успешно добавлен!";
            return RedirectToAction("Flowers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFlower(int id)
        {
            var flower = await _context.Flowers.FirstOrDefaultAsync(f => f.Id == id);

            if (flower == null)
            {
                TempData["ErrorMessage"] = "Цветок не найден!";
                return RedirectToAction("Flowers");
            }

            _context.Flowers.Remove(flower);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Цветок успешно удалён!";
            return RedirectToAction("Flowers");
        }
    }
}