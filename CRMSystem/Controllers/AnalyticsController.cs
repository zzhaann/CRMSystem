using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CRMSystem.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ApplicationDbContext context, ILogger<AnalyticsController> logger) 
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> AnalyzeDay()
        {
            _logger.LogInformation("Analyzing data for today.");

            var today = DateTime.Today;
            var ordersData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date == today)
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new { Hour = g.Key, OrderCount = g.Count(), TotalRevenue = g.Sum(o => o.Price) })
                .OrderBy(g => g.Hour)
                .ToListAsync();

            if (ordersData == null || !ordersData.Any())
            {
                _logger.LogWarning("No completed orders found for today.");
            }

            var topFlowers = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date == today)
                .GroupBy(o => o.FlowerName)
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var topFlorists = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date == today)
                .GroupBy(o => o.Florist.FullName)
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var viewModel = new AnalyticsViewModel
            {
                Labels = ordersData.Select(o => o.Hour + ":00").ToList(),
                OrderCounts = ordersData.Select(o => o.OrderCount).ToList(),
                TotalRevenue = ordersData.Select(o => o.TotalRevenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            _logger.LogInformation("Data for today analyzed successfully.");

            return View(viewModel);
        }

        public async Task<IActionResult> AnalyzeWeek()
        {
            _logger.LogInformation("Analyzing data for the week.");

            var startOfWeek = DateTime.Today.AddDays(-7);
            var endOfWeek = DateTime.Today.AddDays(1);

            var ordersData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfWeek && o.CreatedAt.Date < endOfWeek)
                .GroupBy(o => o.CreatedAt.Date.Date)
                .Select(g => new { Date = g.Key, OrderCount = g.Count(), TotalRevenue = g.Sum(o => o.Price) })
                .OrderBy(g => g.Date)
                .ToListAsync();

            if (ordersData == null || !ordersData.Any())
            {
                _logger.LogWarning("No completed orders found for the week.");
            }

            var topFlowers = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfWeek && o.CreatedAt.Date < endOfWeek)
                .GroupBy(o => o.FlowerName)
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var topFlorists = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfWeek && o.CreatedAt.Date < endOfWeek)
                .GroupBy(o => o.Florist.FullName)
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(3) 
                .ToListAsync();

            var viewModel = new AnalyticsViewModel
            {
                Labels = ordersData.Select(o => o.Date.ToShortDateString()).ToList(),
                OrderCounts = ordersData.Select(o => o.OrderCount).ToList(),
                TotalRevenue = ordersData.Select(o => o.TotalRevenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            _logger.LogInformation("Data for the week analyzed successfully.");

            return View(viewModel); 
        }

        public async Task<IActionResult> AnalyzeMonth()
        {
            _logger.LogInformation("Analyzing data for the month.");

            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var ordersData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfMonth)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, OrderCount = g.Count(), TotalRevenue = g.Sum(o => o.Price) })
                .OrderBy(g => g.Date)
                .ToListAsync();

            if (ordersData == null || !ordersData.Any())
            {
                _logger.LogWarning("No completed orders found for the month.");
            }

            var topFlowers = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfMonth)
                .GroupBy(o => o.FlowerName)
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var topFlorists = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfMonth)
                .GroupBy(o => o.Florist.FullName)
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var viewModel = new AnalyticsViewModel
            {
                Labels = ordersData.Select(o => o.Date.ToShortDateString()).ToList(),
                OrderCounts = ordersData.Select(o => o.OrderCount).ToList(),
                TotalRevenue = ordersData.Select(o => o.TotalRevenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            _logger.LogInformation("Data for the month analyzed successfully.");

            return View(viewModel);
        }

        public async Task<IActionResult> AnalyzeYear()
        {
            _logger.LogInformation("Analyzing data for the year.");

            var startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
            var ordersData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfYear)
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new { Month = g.Key, OrderCount = g.Count(), TotalRevenue = g.Sum(o => o.Price) })
                .OrderBy(g => g.Month)
                .ToListAsync();

            if (ordersData == null || !ordersData.Any())
            {
                _logger.LogWarning("No completed orders found for the year.");
            }

            var topFlowers = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfYear)
                .GroupBy(o => o.FlowerName)
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var topFlorists = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startOfYear)
                .GroupBy(o => o.Florist.FullName)
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var viewModel = new AnalyticsViewModel
            {
                Labels = ordersData.Select(o => $"{o.Month} месяц").ToList(),
                OrderCounts = ordersData.Select(o => o.OrderCount).ToList(),
                TotalRevenue = ordersData.Select(o => o.TotalRevenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            _logger.LogInformation("Data for the year analyzed successfully.");

            return View(viewModel);
        }
    }
}
