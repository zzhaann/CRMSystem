using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FlowersController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public List<Flower> Get()
        {
            return _context.Flowers.ToList();
        }
        [HttpGet("{id}")]
        public Flower Get(int id)
        {
            return _context.Flowers.FirstOrDefault(x => x.Id == id);
        }
        [HttpPost]
        public IActionResult Post([FromForm] Flower flower)
        {
            try
            {
                flower.CreatedAt = DateTime.Now;
                flower.CreatedBy = "Admin";
                _context.Flowers.Add(flower);
                _context.SaveChanges();
                return Ok(new { message = "Flower created successfully", flower });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Flower flower)
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
                    return Ok(new { message = "Flower updated successfully", flower });
                }
                else
                {
                    return NotFound(new { message = "Flower not found" });
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
                var flower = _context.Flowers.FirstOrDefault(x => x.Id == id);
                if (flower != null)
                {
                    _context.Flowers.Remove(flower);
                    _context.SaveChanges();
                    return Ok(new { message = "Flower deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Flower not found" });
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
