using Microsoft.AspNetCore.Mvc;
using CRMSystem.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;
using System;
using System.Drawing;

namespace CRMSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ILogger<ContractsController> _logger;
        private readonly string _apiBaseUrl;

        public ContractsController(ILogger<ContractsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null,
            int? clientId = null, int? floristId = null, int? flowerId = null,
            decimal? minPrice = null, decimal? maxPrice = null, string contractNumber = null)
        {
            _logger.LogInformation("Запрос на получение завершённых заказов с фильтрами.");

            // Если даты не заданы, устанавливаем значения по умолчанию
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ClientId = clientId;
            ViewBag.FloristId = floristId;
            ViewBag.FlowerId = flowerId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.ContractNumber = contractNumber;

            // Загружаем справочники для фильтрации
            await LoadFilterLookups();

            List<Order> completedOrders = await GetCompletedOrders();

            // Применяем фильтры
            completedOrders = FilterOrders(completedOrders, startDate, endDate, clientId,
                floristId, flowerId, minPrice, maxPrice, contractNumber);

            if (!completedOrders.Any())
            {
                _logger.LogWarning("Не найдено завершённых заказов соответствующих фильтрам.");
            }

            var groupedOrders = completedOrders
                .GroupBy(o => o.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            _logger.LogInformation("Завершённые заказы сгруппированы по дате.");

            return View(groupedOrders);
        }

        private async Task LoadFilterLookups()
        {
            // Загрузка клиентов для фильтра
            using (var client = CreateHttpClient())
            {
                try
                {
                    // Получение списка клиентов
                    var clientsResponse = await client.GetAsync($"{_apiBaseUrl}/api/clients");
                    if (clientsResponse.IsSuccessStatusCode)
                    {
                        var clientsJson = await clientsResponse.Content.ReadAsStringAsync();
                        var clients = JsonConvert.DeserializeObject<List<Client>>(clientsJson);
                        ViewBag.Clients = clients;
                    }
                    else
                    {
                        _logger.LogError("Ошибка при получении списка клиентов. Код: {StatusCode}", clientsResponse.StatusCode);
                        ViewBag.Clients = new List<Client>();
                    }

                    // Получение списка флористов
                    var floristsResponse = await client.GetAsync($"{_apiBaseUrl}/api/florists");
                    if (floristsResponse.IsSuccessStatusCode)
                    {
                        var floristsJson = await floristsResponse.Content.ReadAsStringAsync();
                        var florists = JsonConvert.DeserializeObject<List<Florist>>(floristsJson);
                        ViewBag.Florists = florists;
                    }
                    else
                    {
                        _logger.LogError("Ошибка при получении списка флористов. Код: {StatusCode}", floristsResponse.StatusCode);
                        ViewBag.Florists = new List<Florist>();
                    }

                    // Получение списка цветов
                    var flowersResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers");
                    if (flowersResponse.IsSuccessStatusCode)
                    {
                        var flowersJson = await flowersResponse.Content.ReadAsStringAsync();
                        var flowers = JsonConvert.DeserializeObject<List<Flower>>(flowersJson);
                        ViewBag.Flowers = flowers;
                    }
                    else
                    {
                        _logger.LogError("Ошибка при получении списка цветов. Код: {StatusCode}", flowersResponse.StatusCode);
                        ViewBag.Flowers = new List<Flower>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при загрузке справочников для фильтрации");
                    ViewBag.Clients = new List<Client>();
                    ViewBag.Florists = new List<Florist>();
                    ViewBag.Flowers = new List<Flower>();
                }
            }
        }

        private List<Order> FilterOrders(List<Order> orders, DateTime? startDate, DateTime? endDate,
            int? clientId, int? floristId, int? flowerId, decimal? minPrice, decimal? maxPrice,
            string contractNumber)
        {
            // Фильтрация по датам
            if (startDate.HasValue && endDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt.Date >= startDate.Value.Date &&
                                           o.CreatedAt.Date <= endDate.Value.Date)
                               .ToList();
            }

            // Фильтрация по клиенту
            if (clientId.HasValue && clientId.Value > 0)
            {
                orders = orders.Where(o => o.CustomerId == clientId.Value).ToList();
            }

            // Фильтрация по флористу
            if (floristId.HasValue && floristId.Value > 0)
            {
                orders = orders.Where(o => o.FloristId == floristId.Value).ToList();
            }

            // Фильтрация по типу цветов
            if (flowerId.HasValue && flowerId.Value > 0)
            {
                orders = orders.Where(o => o.FlowerId == flowerId.Value).ToList();
            }

            // Фильтрация по минимальной цене
            if (minPrice.HasValue)
            {
                orders = orders.Where(o => o.Price >= minPrice.Value).ToList();
            }

            // Фильтрация по максимальной цене
            if (maxPrice.HasValue)
            {
                orders = orders.Where(o => o.Price <= maxPrice.Value).ToList();
            }

            // Фильтрация по номеру контракта
            if (!string.IsNullOrWhiteSpace(contractNumber))
            {
                orders = orders.Where(o => o.ContractNumber.Contains(contractNumber,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return orders;
        }

        private async Task<List<Order>> GetCompletedOrders()
        {
            List<Order> completedOrders = new List<Order>();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders/completed");
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

            return completedOrders;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? startDate = null, DateTime? endDate = null,
            int? clientId = null, int? floristId = null, int? flowerId = null,
            decimal? minPrice = null, decimal? maxPrice = null, string contractNumber = null)
        {
            _logger.LogInformation("Запрос на экспорт завершённых заказов в Excel с применением фильтров.");

            try
            {
                List<Order> completedOrders = await GetCompletedOrders();

                // Применяем фильтры
                completedOrders = FilterOrders(completedOrders, startDate, endDate, clientId,
                    floristId, flowerId, minPrice, maxPrice, contractNumber);

                // Создаем новую рабочую книгу Excel
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Контракты");

                    // Добавляем заголовки
                    worksheet.Cell(1, 1).Value = "Номер контракта";
                    worksheet.Cell(1, 2).Value = "Цветы";
                    worksheet.Cell(1, 3).Value = "Количество";
                    worksheet.Cell(1, 4).Value = "Цена";
                    worksheet.Cell(1, 5).Value = "Клиент";
                    worksheet.Cell(1, 6).Value = "Телефон";
                    worksheet.Cell(1, 7).Value = "Флорист";
                    worksheet.Cell(1, 8).Value = "Дата создания";

                    // Форматируем заголовки
                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Заполняем данными
                    int row = 2;
                    foreach (var order in completedOrders)
                    {
                        worksheet.Cell(row, 1).Value = order.ContractNumber;
                        worksheet.Cell(row, 2).Value = order.Flower?.Name ?? "";
                        worksheet.Cell(row, 3).Value = order.Flower?.Quantity ?? 0;
                        worksheet.Cell(row, 4).Value = order.Flower?.Price ?? 0;
                        worksheet.Cell(row, 5).Value = order.Customer?.Name ?? "Не указан";
                        worksheet.Cell(row, 6).Value = order.Customer?.Phone ?? "Не указан";
                        worksheet.Cell(row, 7).Value = order.Florist?.FullName ?? "";
                        worksheet.Cell(row, 8).Value = order.CreatedAt.ToString("dd.MM.yyyy");
                        row++;
                    }

                    // Добавляем итоговую строку с суммой
                    row++;
                    worksheet.Cell(row, 3).Value = "Итого:";
                    worksheet.Cell(row, 3).Style.Font.Bold = true;

                    // Суммируем количество
                    var totalQuantity = completedOrders.Sum(o => o.Flower?.Quantity ?? 0);
                    worksheet.Cell(row, 4).Value = totalQuantity;
                    worksheet.Cell(row, 4).Style.Font.Bold = true;

                    // Суммируем цену
                    var totalPrice = completedOrders.Sum(o => o.Flower?.Price ?? 0);
                    worksheet.Cell(row, 5).Value = totalPrice;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;

                    // Автоподбор ширины колонок
                    worksheet.Columns().AdjustToContents();

                    // Сохраняем в память и возвращаем файл
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        string fileName = $"Contracts_{DateTime.Now:yyyy-MM-dd}.xlsx";
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспорте заказов в Excel");
                return RedirectToAction("Index");
            }
        }
    }
}
