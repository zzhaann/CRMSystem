using Microsoft.AspNetCore.Mvc;
using CRMSystem.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRMSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(ILogger<ContractsController> logger)
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

       
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Запрос на получение завершённых заказов.");

            List<Order> completedOrders = new List<Order>();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync("http://localhost:5053/api/orders/completed");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    completedOrders = JsonConvert.DeserializeObject<List<Order>>(json);
                    _logger.LogInformation("Получено {Count} завершённых заказов.", completedOrders.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении завершённых заказов. Код: {StatusCode}", response.StatusCode);
                }
            }

            if (!completedOrders.Any())
            {
                _logger.LogWarning("Не найдено завершённых заказов.");
            }

           
            var groupedOrders = completedOrders
                .GroupBy(o => o.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            _logger.LogInformation("Завершённые заказы сгруппированы по дате.");

            return View(groupedOrders);
        }
    }
}
