using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        [HttpGet]
        public List<Order> Get()
        {
            return _context.Orders.ToList();
        }
        [HttpGet("{id}")]
        public Order Get(int id)
        {
            return _context.Orders.FirstOrDefault(x => x.Id == id);
        }
        [HttpPost]
        public IActionResult Post([FromForm] Order order)
        {
            try
            {
                order.CreatedAt = DateTime.Now;
                order.CreatedBy = "Admin";
                _context.Orders.Add(order);
                _context.SaveChanges();
                return Ok(new { message = "Order created successfully", order });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
        [HttpPut("{id}")]
        public IActionResult Put([FromForm] Order order)
        {
            try
            {
                var existingOrder = _context.Orders.FirstOrDefault(x => x.Id == order.Id);
                if (existingOrder != null)
                {
                    existingOrder.ContractNumber = order.ContractNumber;
                    existingOrder.Quantity = order.Quantity;
                    existingOrder.CustomerName = order.CustomerName;
                    existingOrder.CustomerPhone = order.CustomerPhone;
                    existingOrder.Price = order.Price;
                    existingOrder.FloristId = order.FloristId;
                    existingOrder.FlowerId = order.FlowerId;
                    existingOrder.Status = order.Status;
                    _context.SaveChanges();
                    return Ok(new { message = "Order updated successfully", order });
                }
                else
                {
                    return NotFound(new { message = "Order not found" });
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
                var order = _context.Orders.FirstOrDefault(x => x.Id == id);
                if (order != null)
                {
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                    return Ok(new { message = "Order deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Order not found" });
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
