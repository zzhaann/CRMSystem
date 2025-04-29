using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class FloristsController : Controller
    {
        private readonly ILogger<FloristsController> _logger;
        private readonly AppDbContext _context;
        private HttpClient client = new HttpClient();
        public FloristsController(ILogger<FloristsController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            //var florists = _context.Florists.ToList();
            var florists = new List<Florist>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            using(var response = client.GetAsync("http://localhost:5053/api/florists").Result)
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
                //var florist = _context.Florists.Find(id);
                var florist = new Florist();
                var token = Request.Cookies["jwtToken"];
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                using (var response = client.GetAsync($"http://localhost:5053/api/florists/{id}").Result)
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
                    // Обновление существующего флориста
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(florist),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PutAsync($"http://localhost:5053/api/florists/{florist.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update florist with ID {Id}. Status code: {StatusCode}", florist.Id, response.StatusCode);
                            return View(florist); // Возврат на форму с ошибкой
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating florist {Id}: {Message}", florist.Id, ex.Message);
                    return View(florist); // Возврат на форму с ошибкой
                }
            }
            else
            {
                // Создание нового флориста
                florist.CreatedAt = DateTime.Now;
                florist.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(florist),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PostAsync("http://localhost:5053/api/florists", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create florist. Status code: {StatusCode}", response.StatusCode);
                            return View(florist); // Возврат на форму с ошибкой
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating florist: {Message}", ex.Message);
                    return View(florist); // Возврат на форму с ошибкой
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
                using(var response = client.DeleteAsync($"http://localhost:5053/api/florists/{id}").Result)
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
            catch(Exception ex)
            {
                _logger.LogError("Error occurred while deleting florist with ID {Id}: {Message}", id, ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
