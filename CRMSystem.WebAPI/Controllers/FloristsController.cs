using CRMSystem.WebAPI.Models;
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
    public class FloristsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FloristsController> _logger;

        public FloristsController(AppDbContext appDbContext, ILogger<FloristsController> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        // GET: api/florists
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var florists = _context.Florists.ToList();
                _logger.LogInformation("Fetching all florists.");
                if (florists == null || !florists.Any())
                {
                    _logger.LogWarning("No florists found.");
                    return NotFound(new { message = "No florists found." });
                }
                return Ok(florists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching florists.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/florists/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var florist = _context.Florists.FirstOrDefault(f => f.Id == id);
                if (florist == null)
                {
                    _logger.LogWarning("Florist with ID {Id} not found.", id);
                    return NotFound(new { message = "Florist not found" });
                }
                return Ok(florist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching florist with ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/florists
        [HttpPost]
        public IActionResult Post([FromBody] Florist florist)
        {
            if (florist == null)
            {
                _logger.LogError("Florist data is null.");
                return BadRequest(new { message = "Florist data is required" });
            }

            try
            {
                florist.CreatedAt = DateTime.Now;
                florist.CreatedBy = "Admin";
                _context.Florists.Add(florist);
                _context.SaveChanges();
                _logger.LogInformation("Florist created successfully: {@Florist}", florist);
                return Ok(new { message = "Florist created successfully", florist });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a florist.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        // PUT: api/florists/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Florist florist)
        {
            if (florist == null)
            {
                _logger.LogError("Florist data is null.");
                return BadRequest(new { message = "Florist data is required" });
            }

            try
            {
                var existingFlorist = _context.Florists.FirstOrDefault(f => f.Id == id);
                if (existingFlorist != null)
                {
                    existingFlorist.FullName = florist.FullName;
                    existingFlorist.Phone = florist.Phone;
                    _context.SaveChanges();
                    _logger.LogInformation("Florist with ID {Id} updated successfully: {@Florist}", id, florist);
                    return Ok(new { message = "Florist updated successfully", florist });
                }
                else
                {
                    _logger.LogWarning("Florist with ID {Id} not found.", id);
                    return NotFound(new { message = "Florist not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating florist with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        // DELETE: api/florists/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var florist = _context.Florists.FirstOrDefault(f => f.Id == id);
                if (florist != null)
                {
                    _context.Florists.Remove(florist);
                    _context.SaveChanges();
                    _logger.LogInformation("Florist with ID {Id} deleted successfully.", id);
                    return Ok(new { message = "Florist deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("Florist with ID {Id} not found for deletion.", id);
                    return NotFound(new { message = "Florist not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting florist with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }
    }
}
