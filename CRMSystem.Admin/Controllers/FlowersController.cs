using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class FlowersController : Controller
    {
        private readonly ILogger<FlowersController> _logger;
        private readonly string _apiBaseUrl;
        private readonly HttpClient client = new HttpClient();

        public FlowersController(ILogger<FlowersController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public async Task<IActionResult> Index()
        {
            var flowers = new List<Flower>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using (var response = await client.GetAsync($"{_apiBaseUrl}/api/flowers"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        flowers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Flower>>(json);
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch flowers. Status code: {StatusCode}", response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching flowers: {Message}", ex.Message);
            }

            return View(flowers);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new FlowerViewModel
            {
                Flower = new Flower(),
                Companies = new List<Company>()
            };

            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using (var companiesResponse = await client.GetAsync($"{_apiBaseUrl}/api/companies"))
                {
                    if (companiesResponse.IsSuccessStatusCode)
                    {
                        var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                        viewModel.Companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Company>>(companiesJson);
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch companies. Status code: {StatusCode}", companiesResponse.StatusCode);
                    }
                }

                if (id != 0)
                {
                    using (var flowerResponse = await client.GetAsync($"{_apiBaseUrl}/api/flowers/{id}"))
                    {
                        if (flowerResponse.IsSuccessStatusCode)
                        {
                            var flowerJson = await flowerResponse.Content.ReadAsStringAsync();
                            viewModel.Flower = Newtonsoft.Json.JsonConvert.DeserializeObject<Flower>(flowerJson);
                        }
                        else
                        {
                            _logger.LogError("Failed to fetch flower with ID {Id}. Status code: {StatusCode}", id, flowerResponse.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while preparing the Edit view: {Message}", ex.Message);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FlowerViewModel viewModel)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var flower = viewModel.Flower;

            if (flower.Id != 0)
            {
                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(flower),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PutAsync($"{_apiBaseUrl}/api/flowers/{flower.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update flower with ID {Id}. Status code: {StatusCode}", flower.Id, response.StatusCode);

                            // Заново загружаем список компаний перед возвратом представления
                            await LoadCompaniesForViewModel(viewModel);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating flower with ID {Id}: {Message}", flower.Id, ex.Message);

                    // Заново загружаем список компаний перед возвратом представления
                    await LoadCompaniesForViewModel(viewModel);
                    return View(viewModel);
                }
            }
            else
            {
                flower.CreatedAt = DateTime.Now;
                flower.CreatedBy = User.Identity?.Name ?? "Unknown";
                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(flower),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PostAsync($"{_apiBaseUrl}/api/flowers", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            _logger.LogError("Failed to create flower. Status code: {StatusCode}, {Error}", response.StatusCode, error);

                            // Заново загружаем список компаний перед возвратом представления
                            await LoadCompaniesForViewModel(viewModel);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating flower: {Message}", ex.Message);

                    // Заново загружаем список компаний перед возвратом представления
                    await LoadCompaniesForViewModel(viewModel);
                    return View(viewModel);
                }
            }

            return RedirectToAction("Index");
        }

        // Вспомогательный метод для загрузки компаний
        private async Task LoadCompaniesForViewModel(FlowerViewModel viewModel)
        {
            // Инициализируем Companies пустым списком, если он null
            viewModel.Companies ??= new List<Company>();

            try
            {
                var token = Request.Cookies["jwtToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var companiesResponse = await client.GetAsync($"{_apiBaseUrl}/api/companies"))
                {
                    if (companiesResponse.IsSuccessStatusCode)
                    {
                        var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                        var companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Company>>(companiesJson);
                        if (companies != null)
                        {
                            viewModel.Companies = companies;
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch companies. Status code: {StatusCode}", companiesResponse.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while loading companies: {Message}", ex.Message);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using (var response = await client.DeleteAsync($"{_apiBaseUrl}/api/flowers/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Flower with ID {Id} deleted successfully.", id);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete flower with ID {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while deleting flower with ID {Id}: {Message}", id, ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
