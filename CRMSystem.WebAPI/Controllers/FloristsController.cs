using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpGet]
        public List<Florist> Get()
        {
            _logger.LogInformation("Fetching all florists.");
            return _context.Florists.ToList();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation("Fetching florist with ID {Id}.", id);
            var florist = _context.Florists.FirstOrDefault(f => f.Id == id);
            if (florist == null)
            {
                _logger.LogWarning("Florist with ID {Id} not found.", id);
                return NotFound();
            }
            return Ok(florist);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Florist florist)
        {
            try
            {
                _context.Florists.Add(florist);
                _context.SaveChanges();
                _logger.LogInformation("Florist created successfully: {@Florist}", florist);
                return Ok(new { message = "Florist created successfully", florist });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a florist.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Florist florist)
        {
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
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }

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
                    _logger.LogWarning("Florist with ID {Id} not found.", id);
                    return NotFound(new { message = "Florist not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting florist with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
    }
}
