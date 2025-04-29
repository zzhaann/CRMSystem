using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Получение всех заказов с флористами и цветами...");
            var orders = _context.Orders
                .Include(o => o.Florist)
                .Include(o => o.Flower)
                .ToList();
            return Ok(orders);
        }

       
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation("Получение заказа ID={Id} с деталями...", id);
            var order = _context.Orders
                .Include(o => o.Florist)
                .Include(o => o.Flower)
                .FirstOrDefault(x => x.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Заказ с ID={Id} не найден.", id);
                return NotFound();
            }

            return Ok(order);
        }

        
        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();

                _logger.LogInformation("Заказ успешно создан: {@Order}", order);
                return Ok(new { message = "Order created successfully", order });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                _logger.LogError(ex, "Ошибка при создании заказа: {Message}", innerMessage);
                return BadRequest(ex.Message + " innerMessage: " + innerMessage);
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

                    _logger.LogInformation("Заказ ID={Id} обновлён: {@Order}", order.Id, order);
                    return Ok(new { message = "Order updated successfully", order });
                }
                else
                {
                    _logger.LogWarning("Заказ ID={Id} не найден.", order.Id);
                    return NotFound(new { message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                _logger.LogError(ex, "Ошибка при обновлении заказа ID={Id}: {Message}", order.Id, innerMessage);
                return BadRequest(ex.Message + " innerMessage: " + innerMessage);
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
                    _logger.LogInformation("Заказ ID={Id} удалён.", id);
                    return Ok(new { message = "Order deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("Заказ ID={Id} не найден для удаления.", id);
                    return NotFound(new { message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                _logger.LogError(ex, "Ошибка при удалении заказа ID={Id}: {Message}", id, innerMessage);
                return BadRequest(ex.Message + " innerMessage: " + innerMessage);
            }
        }


        [HttpGet("incoming")]
        public IActionResult GetIncomingOrders()
        {
            _logger.LogInformation("Получение входящих заказов...");
            var orders = _context.Orders
                .Include(o => o.Florist)
                .Include(o => o.Flower)
                .Where(o => o.Status == "Incoming")
                .ToList();
            return Ok(orders);
        }

        
        [HttpGet("processing")]
        public IActionResult GetProcessingOrders()
        {
            _logger.LogInformation("Получение заказов со статусом Processing...");
            var orders = _context.Orders
                .Include(o => o.Florist)
                .Include(o => o.Flower)
                .Where(o => o.Status == "Processing")
                .ToList();
            return Ok(orders);
        }

        
        [HttpGet("completed")]
        public IActionResult GetCompletedOrders()
        {
            _logger.LogInformation("Получение завершённых заказов...");
            var orders = _context.Orders
                .Include(o => o.Florist)
                .Include(o => o.Flower)
                .Where(o => o.Status == "Completed")
                .ToList();
            return Ok(orders);
        }
    }
}
