using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ILogger<ClientsController> _logger;
        private readonly string _apiBaseUrl;
        private HttpClient client = new HttpClient();

        public ClientsController(ILogger<ClientsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public IActionResult Index()
        {
            var clients = new List<Client>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            using (var response = client.GetAsync($"{_apiBaseUrl}/api/clients").Result)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                clients = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Client>>(json);
            }
            return View(clients);
        }

        public IActionResult Edit(int id)
        {
            if (id != 0)
            {
                var client = new Client();
                var token = Request.Cookies["jwtToken"];
                this.client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                using (var response = this.client.GetAsync($"{_apiBaseUrl}/api/clients/{id}").Result)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    client = Newtonsoft.Json.JsonConvert.DeserializeObject<Client>(json);
                }
                return View(client);
            }
            return View(new Client());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Client client)
        {
            var token = Request.Cookies["jwtToken"];
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (client.Id != 0)
            {
                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(client),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await this.client.PutAsync($"{_apiBaseUrl}/api/clients/{client.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update client with ID {Id}. Status code: {StatusCode}", client.Id, response.StatusCode);
                            return View(client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating client {Id}: {Message}", client.Id, ex.Message);
                    return View(client);
                }
            }
            else
            {
                client.CreatedAt = DateTime.Now;
                client.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(client),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await this.client.PostAsync($"{_apiBaseUrl}/api/clients", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create client. Status code: {StatusCode}", response.StatusCode);
                            return View(client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating client: {Message}", ex.Message);
                    return View(client);
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
                using (var response = client.DeleteAsync($"{_apiBaseUrl}/api/clients/{id}").Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Client with ID {Id} deleted successfully.", id);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete client with ID {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while deleting client with ID {Id}: {Message}", id, ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
