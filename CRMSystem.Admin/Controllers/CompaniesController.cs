using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ILogger<FloristsController> _logger;
        private readonly string _apiBaseUrl;
        private HttpClient client = new HttpClient();

        public CompaniesController(ILogger<FloristsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public async Task<IActionResult> Index()
        {
            var companies = new List<Company>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using (var response = await client.GetAsync($"{_apiBaseUrl}/api/companies"))
            {
                var json = await response.Content.ReadAsStringAsync();
                companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Company>>(json);
            }
            return View(companies);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id != 0)
            {
                var company = new Company();
                var token = Request.Cookies["jwtToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var response = await client.GetAsync($"{_apiBaseUrl}/api/companies/{id}"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    company = Newtonsoft.Json.JsonConvert.DeserializeObject<Company>(json);
                }
                return View(company);
            }
            return View(new Company());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Company company)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (company.Id != 0)
            {
                try
                {
                    // Получение существующей компании
                    using (var response = await client.GetAsync($"{_apiBaseUrl}/api/companies/{company.Id}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var existingCompany = Newtonsoft.Json.JsonConvert.DeserializeObject<Company>(json);

                            if (existingCompany != null)
                            {
                                existingCompany.Name = company.Name;
                                existingCompany.Address = company.Address;
                                existingCompany.ContactPhone = company.ContactPhone;

                                var content = new StringContent(
                                    Newtonsoft.Json.JsonConvert.SerializeObject(existingCompany),
                                    System.Text.Encoding.UTF8,
                                    "application/json"
                                );

                                using (var updateResponse = await client.PutAsync($"{_apiBaseUrl}/api/companies/{company.Id}", content))
                                {
                                    if (!updateResponse.IsSuccessStatusCode)
                                    {
                                        _logger.LogError("Failed to update company with ID {Id}. Status code: {StatusCode}", company.Id, updateResponse.StatusCode);
                                        return View(company);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to fetch company with ID {Id}. Status code: {StatusCode}", company.Id, response.StatusCode);
                            return View(company);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while editing company with ID {Id}: {Message}", company.Id, ex.Message);
                    return View(company);
                }
            }
            else
            {
                company.CreatedAt = DateTime.Now;
                company.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(company),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var createResponse = await client.PostAsync($"{_apiBaseUrl}/api/companies", content))
                    {
                        if (!createResponse.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create a new company. Status code: {StatusCode}", createResponse.StatusCode);
                            return View(company);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while creating a new company: {Message}", ex.Message);
                    return View(company);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                using (var response = await client.DeleteAsync($"{_apiBaseUrl}/api/companies/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Company with ID {Id} deleted successfully.", id);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete company with ID {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting company with ID {Id}: {Message}", id, ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
