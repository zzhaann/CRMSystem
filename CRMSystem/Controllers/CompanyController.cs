using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using CRMSystem.Filters;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace CRMSystem.Controllers
{

    [ServiceFilter(typeof(LoggingActionFilter))]
    public class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly string _apiBaseUrl;

        public CompanyController(ILogger<CompanyController> logger, IConfiguration configuration)
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
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Запрос на получение информации для панели управления компанией.");

            using (var client = CreateHttpClient())
            {
                try
                {
                    // Получаем количество клиентов
                    var clientsResponse = await client.GetAsync($"{_apiBaseUrl}/api/clients");
                    if (clientsResponse.IsSuccessStatusCode)
                    {
                        var clientsJson = await clientsResponse.Content.ReadAsStringAsync();
                        var clients = JsonConvert.DeserializeObject<List<Client>>(clientsJson);
                        ViewBag.ClientsCount = clients.Count;
                        // Получаем последних 5 клиентов
                        ViewBag.RecentClients = clients.OrderByDescending(c => c.CreatedAt).Take(5).ToList();
                    }
                    else
                    {
                        ViewBag.ClientsCount = 0;
                        ViewBag.RecentClients = new List<Client>();
                    }

                    // Получаем количество флористов
                    var floristsResponse = await client.GetAsync($"{_apiBaseUrl}/api/florists");
                    if (floristsResponse.IsSuccessStatusCode)
                    {
                        var floristsJson = await floristsResponse.Content.ReadAsStringAsync();
                        var florists = JsonConvert.DeserializeObject<List<Florist>>(floristsJson);
                        ViewBag.FloristsCount = florists.Count;
                    }
                    else
                    {
                        ViewBag.FloristsCount = 0;
                    }

                    // Получаем количество компаний
                    var companiesResponse = await client.GetAsync($"{_apiBaseUrl}/api/companies");
                    if (companiesResponse.IsSuccessStatusCode)
                    {
                        var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                        var companies = JsonConvert.DeserializeObject<List<Company>>(companiesJson);
                        ViewBag.CompaniesCount = companies.Count;
                    }
                    else
                    {
                        ViewBag.CompaniesCount = 0;
                    }

                    // Получаем количество цветов и последние добавленные цветы
                    var flowersResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers");
                    if (flowersResponse.IsSuccessStatusCode)
                    {
                        var flowersJson = await flowersResponse.Content.ReadAsStringAsync();
                        var flowers = JsonConvert.DeserializeObject<List<Flower>>(flowersJson);
                        ViewBag.FlowersCount = flowers.Count;

                        // Получаем последние 5 цветов
                        ViewBag.RecentFlowers = flowers.OrderByDescending(f => f.CreatedAt).Take(5).ToList();

                        // Связываем цветы с компаниями
                        if (ViewBag.RecentFlowers != null)
                        {
                            var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                            var companies = JsonConvert.DeserializeObject<List<Company>>(companiesJson);
                            var companiesDict = companies.ToDictionary(c => c.Id);

                            foreach (var flower in ViewBag.RecentFlowers)
                            {
                                Company company = null;
                                if (flower.CompanyId > 0 && companiesDict.TryGetValue(flower.CompanyId, out company))
                                {
                                    flower.Company = company;
                                }
                            }
                        }
                    }
                    else
                    {
                        ViewBag.FlowersCount = 0;
                        ViewBag.RecentFlowers = new List<Flower>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении данных для панели управления компанией.");
                    TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных.";
                }
            }

            return View();
        }


        public async Task<IActionResult> Clients()
        {
            _logger.LogInformation("Запрос на получение списка клиентов.");

            List<Client> clients = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/clients");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    clients = JsonConvert.DeserializeObject<List<Client>>(json);
                    _logger.LogInformation("Получено {Count} клиентов.", clients.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении клиентов. Код: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при получении списка клиентов.";
                }
            }

            return View(clients);
        }
        [HttpPost]
        public async Task<IActionResult> AddClient(Client client)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Clients");
            }

            _logger.LogInformation("Создание нового клиента: {Name}, {Phone}", client.Name, client.Phone);

            using (var httpClient = CreateHttpClient())
            {
                var json = JsonConvert.SerializeObject(client);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/clients", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Клиент успешно добавлен.");
                    TempData["SuccessMessage"] = "Клиент успешно добавлен!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Конфликт при добавлении клиента: {Response}", responseContent);
                    TempData["ErrorMessage"] = "Клиент с таким номером телефона уже существует!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при добавлении клиента. Код: {StatusCode}. Ответ: {Error}",
                        response.StatusCode, error);
                    TempData["ErrorMessage"] = "Ошибка при добавлении клиента.";
                }
            }

            return RedirectToAction("Clients");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteClient(int id)
        {
            _logger.LogInformation("Удаление клиента с ID={Id}", id);

            using (var httpClient = CreateHttpClient())
            {
                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/clients/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Клиент с ID={Id} успешно удалён.", id);
                    TempData["SuccessMessage"] = "Клиент успешно удален!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Клиент с ID={Id} не найден для удаления.", id);
                    TempData["ErrorMessage"] = "Клиент не найден!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Ошибка при удалении клиента с ID={Id}: {Error}", id, content);
                    TempData["ErrorMessage"] = "Невозможно удалить клиента с существующими заказами!";
                }
                else
                {
                    _logger.LogError("Ошибка при удалении клиента с ID={Id}. Код: {StatusCode}", id, response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при удалении клиента.";
                }
            }

            return RedirectToAction("Clients");
        }

        public async Task<IActionResult> Florists()
        {
            _logger.LogInformation("Запрос на получение списка флористов.");

            List<Florist> florists = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/florists");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    florists = JsonConvert.DeserializeObject<List<Florist>>(json);
                    _logger.LogInformation("Получено {Count} флористов.", florists.Count);
                }
                else
                {
                    _logger.LogError("Ошибка при получении флористов. Код: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при получении списка флористов.";
                }
            }

            return View(florists);
        }
        [HttpPost]
        public async Task<IActionResult> AddFlorist(Florist florist)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Florists");
            }

            _logger.LogInformation("Создание нового флориста: {FullName}, {Phone}", florist.FullName, florist.Phone);

            using (var httpClient = CreateHttpClient())
            {
                var json = JsonConvert.SerializeObject(florist);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/florists", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Флорист успешно добавлен.");
                    TempData["SuccessMessage"] = "Флорист успешно добавлен!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при добавлении флориста. Код: {StatusCode}. Ответ: {Error}",
                        response.StatusCode, error);
                    TempData["ErrorMessage"] = "Ошибка при добавлении флориста.";
                }
            }

            return RedirectToAction("Florists");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteFlorist(int id)
        {
            _logger.LogInformation("Удаление флориста с ID={Id}", id);

            using (var httpClient = CreateHttpClient())
            {
                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/florists/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Флорист с ID={Id} успешно удалён.", id);
                    TempData["SuccessMessage"] = "Флорист успешно удалён!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Флорист с ID={Id} не найден для удаления.", id);
                    TempData["ErrorMessage"] = "Флорист не найден!";
                }
                else
                {
                    _logger.LogError("Ошибка при удалении флориста с ID={Id}. Код: {StatusCode}", id, response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при удалении флориста.";
                }
            }

            return RedirectToAction("Florists");
        }

        public async Task<IActionResult> Companies()
        {
            _logger.LogInformation("Запрос на получение списка компаний.");

            List<Company> companies = new();
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/api/companies");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    companies = JsonConvert.DeserializeObject<List<Company>>(json);
                    _logger.LogInformation("Получено {Count} компаний.", companies.Count);

                    // Получаем цветы для компаний (для отображения связанных цветов)
                    var flowersResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers");
                    if (flowersResponse.IsSuccessStatusCode)
                    {
                        var flowersJson = await flowersResponse.Content.ReadAsStringAsync();
                        var flowers = JsonConvert.DeserializeObject<List<Flower>>(flowersJson);

                        // Группируем цветы по компаниям
                        var flowersGroupedByCompany = flowers.GroupBy(f => f.CompanyId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        // Присваиваем цветы соответствующим компаниям
                        foreach (var company in companies)
                        {
                            if (flowersGroupedByCompany.TryGetValue(company.Id, out var companyFlowers))
                            {
                                company.Flowers = companyFlowers;
                            }
                            else
                            {
                                company.Flowers = new List<Flower>();
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("Ошибка при получении компаний. Код: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при получении списка компаний.";
                }
            }

            return View(companies);
        }
        [HttpPost]
        public async Task<IActionResult> AddCompany(Company company)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Companies");
            }

            _logger.LogInformation("Создание новой компании: {Name}", company.Name);

            using (var httpClient = CreateHttpClient())
            {
                var json = JsonConvert.SerializeObject(company);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/companies", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Компания успешно добавлена.");
                    TempData["SuccessMessage"] = "Компания успешно добавлена!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при добавлении компании. Код: {StatusCode}. Ответ: {Error}",
                        response.StatusCode, error);
                    TempData["ErrorMessage"] = "Ошибка при добавлении компании.";
                }
            }

            return RedirectToAction("Companies");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            _logger.LogInformation("Удаление компании с ID={Id}", id);

            // Сначала проверяем, есть ли у компании цветы
            bool hasFlowers = false;
            using (var httpClient = CreateHttpClient())
            {
                // Получаем список цветов для проверки
                var flowersResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/flowers");
                if (flowersResponse.IsSuccessStatusCode)
                {
                    var flowersJson = await flowersResponse.Content.ReadAsStringAsync();
                    var flowers = JsonConvert.DeserializeObject<List<Flower>>(flowersJson);
                    hasFlowers = flowers.Any(f => f.CompanyId == id);
                }

                if (hasFlowers)
                {
                    _logger.LogWarning("Невозможно удалить компанию с ID={Id}, так как у неё есть цветы.", id);
                    TempData["ErrorMessage"] = "Невозможно удалить компанию с существующими цветами!";
                    return RedirectToAction("Companies");
                }

                // Если цветов нет, удаляем компанию
                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/companies/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Компания с ID={Id} успешно удалена.", id);
                    TempData["SuccessMessage"] = "Компания успешно удалена!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Компания с ID={Id} не найдена для удаления.", id);
                    TempData["ErrorMessage"] = "Компания не найдена!";
                }
                else
                {
                    _logger.LogError("Ошибка при удалении компании с ID={Id}. Код: {StatusCode}", id, response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при удалении компании.";
                }
            }

            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> Flowers()
        {
            _logger.LogInformation("Запрос на получение списка цветов.");

            List<Flower> flowers = new();
            List<Company> companies = new();

            using (var client = CreateHttpClient())
            {
                // Получаем список цветов
                var response = await client.GetAsync($"{_apiBaseUrl}/api/flowers");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    flowers = JsonConvert.DeserializeObject<List<Flower>>(json);
                    _logger.LogInformation("Получено {Count} цветов.", flowers.Count);

                    // Получаем список компаний для выпадающего списка
                    var companiesResponse = await client.GetAsync($"{_apiBaseUrl}/api/companies");
                    if (companiesResponse.IsSuccessStatusCode)
                    {
                        var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                        companies = JsonConvert.DeserializeObject<List<Company>>(companiesJson);
                        _logger.LogInformation("Получено {Count} компаний для выпадающего списка.", companies.Count);

                        // Связываем цветы с компаниями
                        var companiesDict = companies.ToDictionary(c => c.Id);
                        foreach (var flower in flowers)
                        {
                            if (flower.CompanyId > 0 && companiesDict.TryGetValue(flower.CompanyId, out Company company))
                            {
                                flower.Company = company;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Ошибка при получении компаний. Код: {StatusCode}", companiesResponse.StatusCode);
                    }
                }
                else
                {
                    _logger.LogError("Ошибка при получении цветов. Код: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при получении списка цветов.";
                }
            }

            // Передаем список компаний в представление для dropdown-списка
            ViewBag.Companies = companies;

            return View(flowers);
        }
        [HttpPost]
        public async Task<IActionResult> AddFlower(Flower flower)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля.";
                return RedirectToAction("Flowers");
            }

            _logger.LogInformation("Создание нового цветка: {Name}, Количество: {Quantity}, Цена: {Price}",
                flower.Name, flower.Quantity, flower.ClientPrice);

            using (var httpClient = CreateHttpClient())
            {
                var json = JsonConvert.SerializeObject(flower);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/flowers", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Цветок успешно добавлен.");
                    TempData["SuccessMessage"] = "Цветок успешно добавлен!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при добавлении цветка. Код: {StatusCode}. Ответ: {Error}",
                        response.StatusCode, error);
                    TempData["ErrorMessage"] = "Ошибка при добавлении цветка.";
                }
            }

            return RedirectToAction("Flowers");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteFlower(int id)
        {
            _logger.LogInformation("Удаление цветка с ID={Id}", id);

            using (var httpClient = CreateHttpClient())
            {
                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/flowers/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Цветок с ID={Id} успешно удалён.", id);
                    TempData["SuccessMessage"] = "Цветок успешно удалён!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Цветок с ID={Id} не найден для удаления.", id);
                    TempData["ErrorMessage"] = "Цветок не найден!";
                }
                else
                {
                    _logger.LogError("Ошибка при удалении цветка с ID={Id}. Код: {StatusCode}", id, response.StatusCode);
                    TempData["ErrorMessage"] = "Ошибка при удалении цветка.";
                }
            }

            return RedirectToAction("Flowers");
        }
    }
}