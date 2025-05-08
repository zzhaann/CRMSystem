using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Globalization;
using CRMSystem.Models;
using System.Text;

namespace CRMSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly string _apiBaseUrl; // базовый URL из конфигурации

        public DashboardController(ILogger<DashboardController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]; // считываем базовый URL
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Incoming()
        {
            _logger.LogInformation("Запрос на получение заказов со статусом Incoming.");

            List<Order> orders = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/incoming");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<Order>>(json);
                    _logger.LogInformation("Получено {Count} входящих заказов.", orders.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении входящих заказов. Код: {StatusCode}", response.StatusCode);
                }

                var flowersResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers");
                if (flowersResponse.IsSuccessStatusCode)
                {
                    var json = await flowersResponse.Content.ReadAsStringAsync();
                    ViewBag.Flowers = JsonConvert.DeserializeObject<List<Flower>>(json);
                    _logger.LogInformation("Получены данные для: цветы");
                }
                else
                {
                    ViewBag.Flowers = new List<Flower>();
                    _logger.LogError("Ошибка при получении цветов. Код: {StatusCode}", flowersResponse.StatusCode);
                }

                var floristsResponse = await client.GetAsync($"{_apiBaseUrl}/api/florists");
                if (floristsResponse.IsSuccessStatusCode)
                {
                    var json = await floristsResponse.Content.ReadAsStringAsync();
                    ViewBag.Florists = JsonConvert.DeserializeObject<List<Florist>>(json);
                    _logger.LogInformation("Получены данные для: флористы");
                }
                else
                {
                    ViewBag.Florists = new List<Florist>();
                    _logger.LogError("Ошибка при получении флористов. Код: {StatusCode}", floristsResponse.StatusCode);
                }
            }

            return View(orders);
        }

        public async Task<IActionResult> Processing()
        {
            _logger.LogInformation("Запрос на получение заказов со статусом Processing.");

            List<Order> orders = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/processing");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<Order>>(json);
                    _logger.LogInformation("Получено {Count} обрабатываемых заказов.", orders.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении обрабатываемых заказов. Код: {StatusCode}", response.StatusCode);
                }
            }

            return View(orders);
        }

        public async Task<IActionResult> Completed()
        {
            _logger.LogInformation("Запрос на получение заказов со статусом Completed.");

            List<Order> orders = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/completed");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<Order>>(json);
                    _logger.LogInformation("Получено {Count} завершённых заказов.", orders.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении завершённых заказов. Код: {StatusCode}", response.StatusCode);
                }
            }

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            _logger.LogInformation("Создание заказа: ContractNumber={Contract}, Quantity={Quantity}, FloristId={FloristId}, FlowerId={FlowerId}",
                order.ContractNumber, order.Quantity, order.FloristId, order.FlowerId);

            using (var client = CreateHttpClient())
            {
                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBaseUrl}/api/orders", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Заказ успешно создан.");
                    TempData["SuccessMessage"] = "Заказ добавлен!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при создании заказа. Код: {StatusCode}. Тело: {Body}", response.StatusCode, error);
                    TempData["ErrorMessage"] = "Ошибка добавления заказа.";
                }

                return RedirectToAction("Incoming");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoveToProcessing(int id)
        {
            _logger.LogInformation("Перевод заказа ID={Id} в статус Processing.", id);

            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка при получении заказа ID={Id} перед обновлением. Код: {StatusCode}", id, response.StatusCode);
                    return RedirectToAction("Incoming");
                }

                var json = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<Order>(json);
                order.Status = "Processing";

                var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");

                var updateResponse = await client.PutAsync($"{_apiBaseUrl}/api/orders/{id}", content);
                if (updateResponse.IsSuccessStatusCode)
                    _logger.LogInformation("Статус заказа ID={Id} обновлён на Processing.", id);
                else
                    _logger.LogError("Ошибка обновления заказа ID={Id}. Код: {StatusCode}", id, updateResponse.StatusCode);
            }

            return RedirectToAction("Incoming");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCompleted(int id)
        {
            _logger.LogInformation("Перевод заказа ID={Id} в статус Completed.", id);

            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка при получении заказа ID={Id} перед обновлением. Код: {StatusCode}", id, response.StatusCode);
                    return RedirectToAction("Processing");
                }

                var json = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<Order>(json);
                order.Status = "Completed";

                var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");

                var updateResponse = await client.PutAsync($"{_apiBaseUrl}/api/orders/{id}", content);
                if (updateResponse.IsSuccessStatusCode)
                    _logger.LogInformation("Статус заказа ID={Id} обновлён на Completed.", id);
                else
                    _logger.LogError("Ошибка обновления заказа ID={Id}. Код: {StatusCode}", id, updateResponse.StatusCode);
            }

            return RedirectToAction("Processing");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Удаление заказа с ID={Id}", id);

            using (var client = CreateHttpClient())
            {
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/orders/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Заказ ID={Id} успешно удалён.", id);
                    return Ok(new { message = "Order deleted successfully!" });
                }
                else
                {
                    _logger.LogError("Ошибка при удалении заказа ID={Id}. Код: {StatusCode}", id, response.StatusCode);
                    return BadRequest(new { error = "Ошибка удаления заказа" });
                }
            }
        }
    }
}
