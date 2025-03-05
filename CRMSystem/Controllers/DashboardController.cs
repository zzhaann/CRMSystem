using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CRMSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Incoming()
        {
            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Incoming")
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Processing()
        {
            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Processing")
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Completed()
        {
            var orders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Completed")
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            var florist = await _context.Florists.FindAsync(order.FloristId);
            if (florist == null)
            {
                TempData["ErrorMessage"] = "Флорист не найден!";
                return RedirectToAction("Incoming");
            }

            order.Status = "Incoming"; // статусты орнатамыз
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заказ добавлен!";
            return RedirectToAction("Incoming");
        }


        [HttpPost]
        public async Task<IActionResult> MoveToProcessing(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = "Processing";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Incoming");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCompleted(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = "Completed";
                await _context.SaveChangesAsync();
                return RedirectToAction("Processing"); 
            }
            return RedirectToAction("Processing");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Order deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Ошибка удаления заказа: " + ex.Message });
            }
        }



    }
}
