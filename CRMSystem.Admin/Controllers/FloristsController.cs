using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class FloristsController : Controller
    {
        private readonly ILogger<FloristsController> _logger;
        private readonly string _apiBaseUrl;
        private HttpClient client = new HttpClient();

        public FloristsController(ILogger<FloristsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public IActionResult Index()
        {
            var florists = new List<Florist>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            using (var response = client.GetAsync($"{_apiBaseUrl}/api/florists").Result)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                florists = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Florist>>(json);
            }
            return View(florists);
        }

        public IActionResult Edit(int id)
        {
            if (id != 0)
            {
                var florist = new Florist();
                var token = Request.Cookies["jwtToken"];
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                using (var response = client.GetAsync($"{_apiBaseUrl}/api/florists/{id}").Result)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    florist = Newtonsoft.Json.JsonConvert.DeserializeObject<Florist>(json);
                }
                return View(florist);
            }
            return View(new Florist());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Florist florist)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (florist.Id != 0)
            {
                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(florist),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PutAsync($"{_apiBaseUrl}/api/florists/{florist.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update florist with ID {Id}. Status code: {StatusCode}", florist.Id, response.StatusCode);
                            return View(florist);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating florist {Id}: {Message}", florist.Id, ex.Message);
                    return View(florist);
                }
            }
            else
            {
                florist.CreatedAt = DateTime.Now;
                florist.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(florist),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PostAsync($"{_apiBaseUrl}/api/florists", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create florist. Status code: {StatusCode}", response.StatusCode);
                            return View(florist);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating florist: {Message}", ex.Message);
                    return View(florist);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            try
            {
                using (var response = client.DeleteAsync($"{_apiBaseUrl}/api/florists/{id}").Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Florist with ID {Id} deleted successfully.", id);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete florist with ID {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while deleting florist with ID {Id}: {Message}", id, ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
