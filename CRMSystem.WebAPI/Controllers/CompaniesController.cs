using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(AppDbContext appDbContext, ILogger<CompaniesController> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        // GET: api/companies
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var companies = _context.Companies.Include(c => c.Flowers).ToList(); // Включаем цветы в запрос
                _logger.LogInformation("Fetching all companies.");
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching companies.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/companies/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var company = _context.Companies.Include(c => c.Flowers) // Включаем цветы в запрос
                                                .FirstOrDefault(c => c.Id == id);
                if (company == null)
                {
                    _logger.LogWarning("Company with ID {Id} not found.", id);
                    return NotFound(new { message = "Company not found" });
                }
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company by ID.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/companies
        [HttpPost]
        public IActionResult Post([FromBody] CompanyWithFlowerViewModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model is null.");
                return BadRequest(new { message = "Invalid data" });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var company = new Company
                    {
                        Name = model.Name,
                        Address = model.Address,
                        ContactPhone = model.ContactPhone
                    };

                    _context.Companies.Add(company);
                    _context.SaveChanges();

                    var flower = new Flower
                    {
                        Name = model.FlowerName,
                        Quantity = model.Quantity,
                        Price = model.Price,
                        ClientPrice = model.ClientPrice,
                        InitialQuantity = model.Quantity,
                        CompanyId = company.Id
                    };

                    _context.Flowers.Add(flower);
                    _context.SaveChanges();

                    _logger.LogInformation("Company and flower created successfully.");
                    return Ok(new { message = "Company and flower created successfully", company });
                }
                else
                {
                    _logger.LogError("Model is invalid.");
                    return BadRequest(new { message = "Invalid data" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating company and flower.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/companies/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CompanyWithFlowerViewModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model is null.");
                return BadRequest(new { message = "Invalid data" });
            }

            try
            {
                var existingCompany = _context.Companies.FirstOrDefault(c => c.Id == id);
                if (existingCompany != null)
                {
                    existingCompany.Name = model.Name;
                    existingCompany.Address = model.Address;
                    existingCompany.ContactPhone = model.ContactPhone;

                    _context.SaveChanges();

                    var existingFlower = _context.Flowers.FirstOrDefault(f => f.CompanyId == id);
                    if (existingFlower != null)
                    {
                        existingFlower.Name = model.FlowerName;
                        existingFlower.Quantity = model.Quantity;
                        existingFlower.Price = model.Price;
                        existingFlower.ClientPrice = model.ClientPrice;
                        existingFlower.InitialQuantity = model.Quantity;

                        _context.SaveChanges();
                        _logger.LogInformation("Company and flower updated successfully.");
                        return Ok(new { message = "Company and flower updated successfully", existingCompany });
                    }

                    _logger.LogWarning("Flower not found for company ID {Id}.", id);
                    return NotFound(new { message = "Flower not found" });
                }

                _logger.LogWarning("Company with ID {Id} not found.", id);
                return NotFound(new { message = "Company not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating company and flower.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: api/companies/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var company = _context.Companies.FirstOrDefault(c => c.Id == id);
                if (company != null)
                {
                    _context.Companies.Remove(company);
                    _context.SaveChanges();
                    _logger.LogInformation("Company with ID {Id} deleted successfully.", id);
                    return Ok(new { message = "Company deleted successfully" });
                }

                _logger.LogWarning("Company with ID {Id} not found for deletion.", id);
                return NotFound(new { message = "Company not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting company.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
