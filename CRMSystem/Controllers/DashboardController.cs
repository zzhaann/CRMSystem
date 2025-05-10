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

                // Получение списка клиентов
                var clientsResponse = await client.GetAsync($"{_apiBaseUrl}/api/clients");
                if (clientsResponse.IsSuccessStatusCode)
                {
                    var json = await clientsResponse.Content.ReadAsStringAsync();
                    ViewBag.Clients = JsonConvert.DeserializeObject<List<Client>>(json);
                    _logger.LogInformation("Получены данные для: клиенты");
                }
                else
                {
                    ViewBag.Clients = new List<Client>();
                    _logger.LogError("Ошибка при получении клиентов. Код: {StatusCode}", clientsResponse.StatusCode);
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
        public async Task<IActionResult> Create(Order order, string customerName, string customerPhone)
        {
            _logger.LogInformation("Создание заказа: ContractNumber={Contract}, Quantity={Quantity}, FloristId={FloristId}, FlowerId={FlowerId}",
                order.ContractNumber, order.Quantity, order.FloristId, order.FlowerId);

            using (var client = CreateHttpClient())
            {
                // Обработка информации о клиенте
                if (order.CustomerId == null && !string.IsNullOrEmpty(customerPhone))
                {
                    // Поиск клиента по номеру телефона
                    var clientResponse = await client.GetAsync($"{_apiBaseUrl}/api/clients/phone/{customerPhone}");

                    if (clientResponse.IsSuccessStatusCode)
                    {
                        // Клиент найден - используем его ID
                        var clientJson = await clientResponse.Content.ReadAsStringAsync();
                        var existingClient = JsonConvert.DeserializeObject<Client>(clientJson);
                        order.CustomerId = existingClient.Id;
                        _logger.LogInformation("Найден существующий клиент с ID={Id}", existingClient.Id);
                    }
                    else if (clientResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Клиент не найден - создаем нового
                        var newClient = new Client
                        {
                            Name = customerName ?? "",
                            Phone = customerPhone
                        };

                        var clientJson = JsonConvert.SerializeObject(newClient);
                        var clientContent = new StringContent(clientJson, Encoding.UTF8, "application/json");

                        var createClientResponse = await client.PostAsync($"{_apiBaseUrl}/api/clients", clientContent);

                        if (createClientResponse.IsSuccessStatusCode)
                        {
                            var createdClientJson = await createClientResponse.Content.ReadAsStringAsync();
                            _logger.LogInformation("Ответ API при создании клиента: {Response}", createdClientJson);

                            // Десериализация с учетом структуры ответа
                            var responseObject = JsonConvert.DeserializeObject<dynamic>(createdClientJson);
                            var createdClient = JsonConvert.DeserializeObject<Client>(responseObject.client.ToString());

                            if (createdClient != null && createdClient.Id > 0)
                            {
                                order.CustomerId = createdClient.Id;

                                _logger.LogInformation("Создан новый клиент с ID={Id}", (int)createdClient.Id);
                            }
                            else
                            {
                                _logger.LogError("Ошибка: API вернул некорректный ID клиента.");
                                TempData["ErrorMessage"] = "Ошибка создания клиента.";
                                return RedirectToAction("Incoming");
                            }
                        }
                        else
                        {
                            _logger.LogError("Ошибка при создании клиента. Код: {StatusCode}", createClientResponse.StatusCode);
                            TempData["ErrorMessage"] = "Ошибка создания клиента.";
                            return RedirectToAction("Incoming");
                        }
                    }
                }

                Flower flower = null;
                // Получение информации о цветке для расчета цены
                if (order.FlowerId.HasValue)
                {
                    var flowerResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers/{order.FlowerId}");
                    if (flowerResponse.IsSuccessStatusCode)
                    {
                        var flowerJson = await flowerResponse.Content.ReadAsStringAsync();
                        flower = JsonConvert.DeserializeObject<Flower>(flowerJson);

                        // Установка цены заказа на основе цены цветка и количества
                        order.Price = flower.ClientPrice * order.Quantity;
                        _logger.LogInformation("Установлена цена заказа: {Price}", order.Price);

                        // Проверка, достаточно ли цветов в наличии
                        if (flower.Quantity < order.Quantity)
                        {
                            _logger.LogWarning("Недостаточное количество цветов в наличии. Запрошено: {Requested}, Доступно: {Available}",
                                order.Quantity, flower.Quantity);
                            TempData["ErrorMessage"] = "Недостаточное количество цветов в наличии.";
                            return RedirectToAction("Incoming");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось получить информацию о цветке для расчета цены");
                        TempData["ErrorMessage"] = "Не удалось получить информацию о цветке.";
                        return RedirectToAction("Incoming");
                    }
                }

                // Создание заказа
                order.CreatedBy = User.Identity.Name;
                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBaseUrl}/api/orders", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Заказ успешно создан.");

                    // Обновление количества цветов в БД
                    if (flower != null && order.FlowerId.HasValue)
                    {
                        // Уменьшаем количество цветов
                        flower.Quantity -= order.Quantity;

                        // Отправляем запрос на обновление цветка в БД
                        var flowerJson = JsonConvert.SerializeObject(flower);
                        var flowerContent = new StringContent(flowerJson, Encoding.UTF8, "application/json");

                        var updateFlowerResponse = await client.PutAsync($"{_apiBaseUrl}/api/flowers/{flower.Id}", flowerContent);

                        if (updateFlowerResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Количество цветов ID={Id} успешно обновлено. Новое количество: {NewQuantity}",
                                flower.Id, flower.Quantity);
                        }
                        else
                        {
                            _logger.LogError("Ошибка при обновлении количества цветов ID={Id}. Код: {StatusCode}",
                                flower.Id, updateFlowerResponse.StatusCode);
                        }
                    }

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
                // Получаем информацию о заказе перед удалением
                var orderResponse = await client.GetAsync($"{_apiBaseUrl}/api/orders/{id}");
                if (!orderResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка при получении заказа ID={Id} перед удалением. Код: {StatusCode}",
                        id, orderResponse.StatusCode);
                    return BadRequest(new { error = "Ошибка получения информации о заказе" });
                }

                var orderJson = await orderResponse.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<Order>(orderJson);

                // Проверяем, есть ли информация о цветке в заказе
                if (order.FlowerId.HasValue && order.Quantity > 0)
                {
                    // Получаем текущую информацию о цветке
                    var flowerResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers/{order.FlowerId}");
                    if (flowerResponse.IsSuccessStatusCode)
                    {
                        var flowerJson = await flowerResponse.Content.ReadAsStringAsync();
                        var flower = JsonConvert.DeserializeObject<Flower>(flowerJson);

                        // Увеличиваем количество цветов обратно
                        flower.Quantity += order.Quantity;
                        _logger.LogInformation("Возвращение {Quantity} шт. цветов ID={FlowerId} в наличие",
                            order.Quantity, order.FlowerId);

                        // Обновляем информацию о цветке в БД
                        var updatedFlowerJson = JsonConvert.SerializeObject(flower);
                        var flowerContent = new StringContent(updatedFlowerJson, Encoding.UTF8, "application/json");

                        var updateFlowerResponse = await client.PutAsync($"{_apiBaseUrl}/api/flowers/{flower.Id}", flowerContent);

                        if (!updateFlowerResponse.IsSuccessStatusCode)
                        {
                            _logger.LogError("Ошибка при обновлении количества цветов ID={FlowerId}. Код: {StatusCode}",
                                flower.Id, updateFlowerResponse.StatusCode);
                        }
                        else
                        {
                            _logger.LogInformation("Количество цветов ID={FlowerId} успешно обновлено. Новое количество: {NewQuantity}",
                                flower.Id, flower.Quantity);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось получить информацию о цветке ID={FlowerId} для возврата количества",
                            order.FlowerId);
                    }
                }

                // Удаляем заказ
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
