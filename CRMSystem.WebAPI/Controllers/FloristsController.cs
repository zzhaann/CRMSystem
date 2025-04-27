using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FloristsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FloristsController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        [HttpGet]
        public List<Florist> Get()
        {
            return _context.Florists.ToList();
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var florist = _context.Florists.FirstOrDefault(f => f.Id == id);
            if (florist == null)
            {
                return NotFound();
            }
            return Ok(florist);
        }
        [HttpPost]
        public IActionResult Post([FromForm] Florist florist)
        {
            try
            {
                florist.CreatedAt = DateTime.Now;
                florist.CreatedBy = "Admin";
                _context.Florists.Add(florist);
                _context.SaveChanges();
                return Ok(new { message = "Florist created succesfully", florist });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Florist florist)
        {
            try
            {
                var existingFlorist = _context.Florists.FirstOrDefault(f => f.Id == id);
                if (existingFlorist != null)
                {
                    existingFlorist.FullName = florist.FullName;
                    existingFlorist.Phone = florist.Phone;
                    _context.SaveChanges();
                    return Ok(new { message = "Florist updated successfully", florist });
                }
                else
                {
                    return NotFound(new { message = "Florist not found" });
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
            try
            {
                var florist = _context.Florists.FirstOrDefault(f => f.Id == id);
                if (florist != null)
                {
                    _context.Florists.Remove(florist);
                    _context.SaveChanges();
                    return Ok(new { message = "Florist deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Florist not found" });
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
    }
}
