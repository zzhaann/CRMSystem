using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CompaniesController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        [HttpGet]
        public List<Company> Get()
        {
            return _context.Companies.ToList();
        }
        [HttpGet("{id}") ]
        public IActionResult Get(int id)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }
            return Ok(company);
        }
        [HttpPost]
        public IActionResult Post([FromForm] Company company)
        {
            try
            {
                company.CreatedAt = DateTime.Now;
                company.CreatedBy = "Admin";
                _context.Companies.Add(company);
                _context.SaveChanges();
                return Ok(new { message = "Company created successfully", company });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Company company)
        {
            try
            {
                var existingCompany = _context.Companies.FirstOrDefault(f => f.Id == id);
                if (existingCompany != null)
                {
                    existingCompany.Name = company.Name;
                    existingCompany.Address = company.Address;
                    existingCompany.ContactPhone = company.ContactPhone;
                    _context.SaveChanges();
                    return Ok(new { message = "Company updated successfully", company });
                }
                else
                {
                    return NotFound( new { message = "Company not found"});
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var company = _context.Companies.FirstOrDefault(f => f.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            _context.Companies.Remove(company);
            _context.SaveChanges();
            return Ok(new { message = "Company deleted successfully" });
        }

    }
}
