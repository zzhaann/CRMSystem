using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CRMSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var completedOrders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Completed")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var groupedOrders = completedOrders
                .GroupBy(o => o.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(groupedOrders);
        }
    }
}
