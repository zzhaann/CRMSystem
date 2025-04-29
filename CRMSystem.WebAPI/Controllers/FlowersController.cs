using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class FlowersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FlowersController> _logger;

        public FlowersController(AppDbContext context, ILogger<FlowersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/flowers
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Fetching all flowers.");
            var flowers = _context.Flowers.ToList();
            if (flowers == null || !flowers.Any())
            {
                return NotFound(new { message = "No flowers found." });
            }
            return Ok(flowers);
        }

        // GET: api/flowers/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation("Fetching flower with ID {Id}.", id);
            var flower = _context.Flowers.FirstOrDefault(x => x.Id == id);
            if (flower == null)
            {
                return NotFound(new { message = "Flower not found." });
            }
            return Ok(flower);
        }

        // POST: api/flowers
        [HttpPost]
        public IActionResult Post([FromForm] Flower flower)
        {
            try
            {
                flower.CreatedAt = DateTime.Now;
                flower.CreatedBy = "Admin";
                _context.Flowers.Add(flower);
                _context.SaveChanges();
                _logger.LogInformation("Flower created successfully: {@Flower}", flower);
                return CreatedAtAction(nameof(Get), new { id = flower.Id }, flower);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a flower.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        // PUT: api/flowers/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Flower flower)
        {
            try
            {
                var existingFlower = _context.Flowers.FirstOrDefault(x => x.Id == id);
                if (existingFlower != null)
                {
                    existingFlower.Name = flower.Name;
                    existingFlower.Quantity = flower.Quantity;
                    existingFlower.InitialQuantity = flower.InitialQuantity;
                    existingFlower.Price = flower.Price;
                    existingFlower.ClientPrice = flower.ClientPrice;
                    existingFlower.CompanyId = flower.CompanyId;

                    _context.SaveChanges();
                    _logger.LogInformation("Flower with ID {Id} updated successfully: {@Flower}", id, flower);
                    return Ok(new { message = "Flower updated successfully", flower });
                }
                else
                {
                    _logger.LogWarning("Flower with ID {Id} not found.", id);
                    return NotFound(new { message = "Flower not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating flower with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        // DELETE: api/flowers/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var flower = _context.Flowers.FirstOrDefault(x => x.Id == id);
                if (flower != null)
                {
                    _context.Flowers.Remove(flower);
                    _context.SaveChanges();
                    _logger.LogInformation("Flower with ID {Id} deleted successfully.", id);
                    return Ok(new { message = "Flower deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("Flower with ID {Id} not found.", id);
                    return NotFound(new { message = "Flower not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting flower with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }
    }
}
