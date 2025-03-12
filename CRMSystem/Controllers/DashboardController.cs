using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CRMSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Incoming()
        {
            _logger.LogInformation("Fetching incoming orders.");

            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Incoming")
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                _logger.LogInformation("No incoming orders found.");
            }

            return View(orders);
        }

        public async Task<IActionResult> Processing()
        {
            _logger.LogInformation("Fetching processing orders.");

            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Processing")
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                _logger.LogInformation("No processing orders found.");
            }

            return View(orders);
        }

        public async Task<IActionResult> Completed()
        {
            _logger.LogInformation("Fetching completed orders.");

            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Completed")
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                _logger.LogInformation("No completed orders found.");
            }

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            _logger.LogInformation("Creating a new order with contract number: {ContractNumber}", order.ContractNumber);

            var florist = await _context.Florists.FindAsync(order.FloristId);
            if (florist == null)
            {
                _logger.LogWarning("Florist with ID {FloristId} not found.", order.FloristId);
                TempData["ErrorMessage"] = "Флорист не найден!";
                return RedirectToAction("Incoming");
            }

            order.Status = "Incoming"; // статусты орнатамыз
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order with contract number {ContractNumber} created successfully.", order.ContractNumber);
            TempData["SuccessMessage"] = "Заказ добавлен!";
            return RedirectToAction("Incoming");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToProcessing(int id)
        {
            _logger.LogInformation("Moving order with ID {OrderId} to processing.", id);

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = "Processing";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order with ID {OrderId} moved to processing.", id);
            }
            else
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
            }

            return RedirectToAction("Incoming");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCompleted(int id)
        {
            _logger.LogInformation("Moving order with ID {OrderId} to completed.", id);

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = "Completed";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order with ID {OrderId} moved to completed.", id);
                return RedirectToAction("Processing");
            }
            else
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
            }

            return RedirectToAction("Processing");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting order with ID {OrderId}.", id);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
                return NotFound();
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order with ID {OrderId} deleted successfully.", id);
                return Ok(new { message = "Order deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting order with ID {OrderId}: {Error}", id, ex.Message);
                return BadRequest(new { error = "Ошибка удаления заказа: " + ex.Message });
            }
        }
    }
}
