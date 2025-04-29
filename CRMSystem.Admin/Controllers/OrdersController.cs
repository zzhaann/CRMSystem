using CRMSystem.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CRMSystem.Admin.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly HttpClient client = new HttpClient();

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var orders = new List<Order>();
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using (var response = await client.GetAsync("http://localhost:5053/api/orders"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        orders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(json);
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch orders. Status code: {StatusCode}", response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching orders: {Message}", ex.Message);
            }

            return View(orders);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new OrderViewModel
            {
                Order = new Order(),
                Flowers = new List<Flower>(),
                Florists = new List<Florist>()
            };

            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Получение списка цветов
                using (var flowersResponse = await client.GetAsync("http://localhost:5053/api/flowers"))
                {
                    if (flowersResponse.IsSuccessStatusCode)
                    {
                        var flowersJson = await flowersResponse.Content.ReadAsStringAsync();
                        viewModel.Flowers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Flower>>(flowersJson);
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch flowers. Status code: {StatusCode}", flowersResponse.StatusCode);
                    }
                }

                // Получение списка флористов
                using (var floristsResponse = await client.GetAsync("http://localhost:5053/api/florists"))
                {
                    if (floristsResponse.IsSuccessStatusCode)
                    {
                        var floristsJson = await floristsResponse.Content.ReadAsStringAsync();
                        viewModel.Florists = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Florist>>(floristsJson);
                    }
                    else
                    {
                        _logger.LogError("Failed to fetch florists. Status code: {StatusCode}", floristsResponse.StatusCode);
                    }
                }

                // Если редактируем существующий заказ
                if (id != 0)
                {
                    using (var orderResponse = await client.GetAsync($"http://localhost:5053/api/orders/{id}"))
                    {
                        if (orderResponse.IsSuccessStatusCode)
                        {
                            var orderJson = await orderResponse.Content.ReadAsStringAsync();
                            viewModel.Order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(orderJson);
                        }
                        else
                        {
                            _logger.LogError("Failed to fetch order with ID {Id}. Status code: {StatusCode}", id, orderResponse.StatusCode);
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
        public async Task<IActionResult> Edit(OrderViewModel viewModel)
        {
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var order = viewModel.Order;

            if (order.Id != 0)
            {
                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(order),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PutAsync($"http://localhost:5053/api/orders/{order.Id}", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to update order with ID {Id}. Status code: {StatusCode}", order.Id, response.StatusCode);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while updating order with ID {Id}: {Message}", order.Id, ex.Message);
                    return View(viewModel);
                }
            }
            else
            {
                order.CreatedAt = DateTime.Now;
                order.CreatedBy = User.Identity.Name;

                try
                {
                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(order),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    using (var response = await client.PostAsync("http://localhost:5053/api/orders", content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Failed to create order. Status code: {StatusCode}", response.StatusCode);
                            return View(viewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while creating order: {Message}", ex.Message);
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
                using (var response = await client.DeleteAsync($"http://localhost:5053/api/orders/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Order with ID {Id} deleted successfully.", id);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete order with ID {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while deleting order with ID {Id}: {Message}", id, ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
