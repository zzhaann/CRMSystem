using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class FlowersController : Controller
    {
        private readonly ILogger<FlowersController> _logger;
        private readonly HttpClient client = new HttpClient();

        public FlowersController(ILogger<FlowersController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var flowers = new List<Flower>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using (var response = await client.GetAsync("http://localhost:5053/api/flowers"))
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
                using (var companiesResponse = await client.GetAsync("http://localhost:5053/api/companies"))
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
                    using (var flowerResponse = await client.GetAsync($"http://localhost:5053/api/flowers/{id}"))
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

                    using (var response = await client.PutAsync($"http://localhost:5053/api/flowers/{flower.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update flower with ID {Id}. Status code: {StatusCode}", flower.Id, response.StatusCode);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating flower with ID {Id}: {Message}", flower.Id, ex.Message);
                    return View(viewModel);
                }
            }
            else
            {
                flower.CreatedAt = DateTime.Now;
                flower.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(flower),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PostAsync("http://localhost:5053/api/flowers", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create flower. Status code: {StatusCode}", response.StatusCode);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating flower: {Message}", ex.Message);
                    return View(viewModel);
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
                using (var response = await client.DeleteAsync($"http://localhost:5053/api/flowers/{id}"))
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
