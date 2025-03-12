using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CRMSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(ApplicationDbContext context, ILogger<ContractsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching completed orders.");

            var completedOrders = await _context.Orders
                .Include(o => o.Florist)
                .Where(o => o.Status == "Completed")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            if (completedOrders == null || !completedOrders.Any())
            {
                _logger.LogWarning("No completed orders found.");
            }

            var groupedOrders = completedOrders
                .GroupBy(o => o.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            _logger.LogInformation("Completed orders fetched and grouped by date.");

            return View(groupedOrders);
        }
    }
}
