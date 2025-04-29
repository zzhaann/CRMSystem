using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Globalization;
using CRMSystem.Models;

namespace CRMSystem.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ILogger<AnalyticsController> logger)
        {
            _logger = logger;
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        
        public async Task<IActionResult> AnalyzeDay()
        {
            _logger.LogInformation("Запрос аналитики за сегодня.");

            using var client = CreateHttpClient();
            var response = await client.GetAsync("http://localhost:5053/api/orders/completed");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Не удалось получить завершённые заказы. Код: {StatusCode}", response.StatusCode);
                return View(new AnalyticsViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(json);
            var today = DateTime.Today;

            var todayOrders = orders
                .Where(o => o.CreatedAt.Date == today)
                .ToList();

            var hourlyGroups = todayOrders
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.Price)
                })
                .OrderBy(g => g.Hour)
                .ToList();

            var topFlowers = todayOrders
                .GroupBy(o => o.Flower?.Name ?? "Неизвестно")
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            var topFlorists = todayOrders
                .GroupBy(o => o.Florist?.FullName ?? "Неизвестно")
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            var viewModel = new AnalyticsViewModel
            {
                Labels = hourlyGroups.Select(g => $"{g.Hour}:00").ToList(),
                OrderCounts = hourlyGroups.Select(g => g.OrderCount).ToList(),
                TotalRevenue = hourlyGroups.Select(g => g.TotalRevenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AnalyzeWeek()
        {
            return await AnalyzeByRange(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1), "день");
        }

        
        public async Task<IActionResult> AnalyzeMonth()
        {
            var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return await AnalyzeByRange(start, DateTime.Today.AddDays(1), "день");
        }

       
        public async Task<IActionResult> AnalyzeYear()
        {
            var start = new DateTime(DateTime.Today.Year, 1, 1);
            return await AnalyzeByRange(start, DateTime.Today.AddDays(1), "месяц");
        }

        
        private async Task<IActionResult> AnalyzeByRange(DateTime start, DateTime end, string groupBy)
        {
            _logger.LogInformation("Анализ с {Start} по {End}, группировка по {GroupBy}", start, end, groupBy);

            using var client = CreateHttpClient();
            var response = await client.GetAsync("http://localhost:5053/api/orders/completed");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Не удалось получить завершённые заказы. Код: {StatusCode}", response.StatusCode);
                return View("AnalyzeDay", new AnalyticsViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(json);

            var filtered = orders
                .Where(o => o.CreatedAt.Date >= start && o.CreatedAt.Date < end)
                .ToList();

            var grouped = groupBy == "день"
                ? filtered.GroupBy(o => o.CreatedAt.Date)
                          .Select(g => new { Label = g.Key.ToShortDateString(), Count = g.Count(), Revenue = g.Sum(o => o.Price) })
                : filtered.GroupBy(o => o.CreatedAt.Month)
                          .Select(g => new { Label = $"{g.Key} месяц", Count = g.Count(), Revenue = g.Sum(o => o.Price) });

            var topFlowers = filtered
                .GroupBy(o => o.Flower?.Name ?? "Неизвестно")
                .Select(g => new { Flower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            var topFlorists = filtered
                .GroupBy(o => o.Florist?.FullName ?? "Неизвестно")
                .Select(g => new { Florist = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            var viewModel = new AnalyticsViewModel
            {
                Labels = grouped.Select(g => g.Label).ToList(),
                OrderCounts = grouped.Select(g => g.Count).ToList(),
                TotalRevenue = grouped.Select(g => g.Revenue).ToList(),
                TopFlowers = topFlowers.Select(f => f.Flower).ToList(),
                FlowerCounts = topFlowers.Select(f => f.Count).ToList(),
                TopFlorists = topFlorists.Select(f => f.Florist).ToList(),
                FloristOrders = topFlorists.Select(f => f.Count).ToList()
            };

            return View("AnalyzeDay", viewModel);
        }
    }
}
