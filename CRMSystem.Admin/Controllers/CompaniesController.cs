using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.Admin.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ILogger<FloristsController> _logger;
        private readonly AppDbContext _context;
        public CompaniesController(ILogger<FloristsController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            var companies = _context.Companies.ToList();
            return View(companies);
        }
        public IActionResult Edit(int id)
        {
            if (id != 0)
            {
                var company = _context.Companies.Find(id);
                return View(company);
            }
            return View(new Company());
        }
        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (company.Id != 0)
            {
                var _company = _context.Companies.Find(company.Id);
                if (_company != null)
                {
                    _company.Name = company.Name;
                    _company.Address = company.Address;
                    _company.ContactPhone = company.ContactPhone;
                }
            }
            else
            {
                company.CreatedAt = DateTime.Now;
                company.CreatedBy = User.Identity.Name;
                _context.Companies.Add(company);
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var company = _context.Companies.Find(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
