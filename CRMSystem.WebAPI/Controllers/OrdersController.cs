using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext appDbContext, ILogger<OrdersController> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        [HttpGet]
        public List<Order> Get()
        {
            _logger.LogInformation("Fetching all orders.");
            return _context.Orders.ToList();
        }

        [HttpGet("{id}")]
        public Order Get(int id)
        {
            _logger.LogInformation("Fetching order with ID {Id}.", id);
            return _context.Orders.FirstOrDefault(x => x.Id == id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
                _logger.LogInformation("Order created successfully: {@Order}", order);
                return Ok(new { message = "Order created successfully", order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating an order.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Order order)
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
                    _logger.LogInformation("Order with ID {Id} updated successfully: {@Order}", order.Id, order);
                    return Ok(new { message = "Order updated successfully", order });
                }
                else
                {
                    _logger.LogWarning("Order with ID {Id} not found.", order.Id);
                    return NotFound(new { message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID {Id}.", order.Id);
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
                    _logger.LogInformation("Order with ID {Id} deleted successfully.", id);
                    return Ok(new { message = "Order deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("Order with ID {Id} not found.", id);
                    return NotFound(new { message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(ex.Message + "innerMessage: " + innerMessage);
            }
        }
    }
}
