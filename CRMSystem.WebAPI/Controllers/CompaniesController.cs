using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(AppDbContext appDbContext, ILogger<CompaniesController> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        [HttpGet]
        public List<Company> Get()
        {
            _logger.LogInformation("Fetching all companies.");
            return _context.Companies.ToList();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation("Fetching company with ID {Id}.", id);
            var company = _context.Companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                _logger.LogWarning("Company with ID {Id} not found.", id);
                return NotFound(new { message = "Company not found" });
            }
            return Ok(company);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Company company)
        {
            try
            {
                _context.Companies.Add(company);
                _context.SaveChanges();
                _logger.LogInformation("Company created successfully: {@Company}", company);
                return Ok(new { message = "Company created successfully", company });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a company.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Company company)
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
                    _logger.LogInformation("Company with ID {Id} updated successfully: {@Company}", id, company);
                    return Ok(new { message = "Company updated successfully", company });
                }
                else
                {
                    _logger.LogWarning("Company with ID {Id} not found.", id);
                    return NotFound(new { message = "Company not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating company with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var company = _context.Companies.FirstOrDefault(f => f.Id == id);
                if (company == null)
                {
                    _logger.LogWarning("Company with ID {Id} not found.", id);
                    return NotFound();
                }
                _context.Companies.Remove(company);
                _context.SaveChanges();
                _logger.LogInformation("Company with ID {Id} deleted successfully.", id);
                return Ok(new { message = "Company deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting company with ID {Id}.", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
