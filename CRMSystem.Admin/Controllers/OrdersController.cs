using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly AppDbContext _context;
        public OrdersController(ILogger<OrdersController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders.Include(o => o.Flower).Include(o => o.Florist).ToList();

            return View(orders);
        }
        public IActionResult Edit(int id)
        {
            var ovm = new OrderViewModel();
            ovm.Florists = _context.Florists.ToList();
            ovm.Flowers = _context.Flowers.ToList();
            ovm.Order = new Order();
            if (id != 0)
            {
                var order = _context.Orders.Find(id);
                if (order != null)
                {
                    ovm.Order = order;
                    return View(ovm);
                }
            }
            return View(ovm);
        }
        [HttpPost]
        public IActionResult edit(Order order)
        {
            if (order.Id != 0)
            {
                var _order = _context.Orders.Find(order.Id);
                if(_order != null)
                {
                    _order.ContractNumber = order.ContractNumber;
                    _order.Quantity = order.Quantity;
                    _order.CustomerName = order.CustomerName;
                    _order.CustomerPhone = order.CustomerPhone;
                    _order.Price = order.Price;
                    _order.FloristId = order.FloristId;
                    _order.FlowerId = order.FlowerId;
                    _order.Status = order.Status;
                    _context.SaveChanges();
                }
            }
            else
            {
                order.CreatedAt = DateTime.Now;
                order.CreatedBy = User.Identity.Name;
                _context.Orders.Add(order);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
    }
}
