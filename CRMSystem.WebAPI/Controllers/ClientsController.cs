using CRMSystem.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRMSystem.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(AppDbContext appDbContext, ILogger<ClientsController> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var clients = _context.Clients.ToList();
                _logger.LogInformation("Получение всех клиентов.");
                if (clients == null || !clients.Any())
                {
                    _logger.LogWarning("Клиенты не найдены.");
                    return NotFound(new { message = "Клиенты не найдены." });
                }
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка клиентов.");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var client = _context.Clients.FirstOrDefault(c => c.Id == id);
                if (client == null)
                {
                    _logger.LogWarning("Клиент с ID {Id} не найден.", id);
                    return NotFound(new { message = "Клиент не найден" });
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента с ID {Id}.", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("phone/{phone}")]
        public IActionResult GetByPhone(string phone)
        {
            try
            {
                var client = _context.Clients.FirstOrDefault(c => c.Phone == phone);
                if (client == null)
                {
                    _logger.LogWarning("Клиент с номером телефона {Phone} не найден.", phone);
                    return NotFound(new { message = "Клиент не найден" });
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента по номеру телефона {Phone}.", phone);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {
            if (client == null)
            {
                _logger.LogError("Данные клиента отсутствуют.");
                return BadRequest(new { message = "Требуются данные клиента" });
            }

            try
            {
                // Проверка существования клиента с таким же номером телефона
                var existingClient = _context.Clients.FirstOrDefault(c => c.Phone == client.Phone);
                if (existingClient != null)
                {
                    _logger.LogWarning("Клиент с номером телефона {Phone} уже существует.", client.Phone);
                    return Conflict(new { message = "Клиент с таким номером телефона уже существует", client = existingClient });
                }

                client.CreatedAt = DateTime.Now;
                _context.Clients.Add(client);
                _context.SaveChanges();
                _logger.LogInformation("Клиент успешно создан: {@Client}", client);
                return Ok(new { message = "Клиент успешно создан", client });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании клиента.");
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Client client)
        {
            try
            {
                var existingClient = _context.Clients.FirstOrDefault(c => c.Id == id);
                if (existingClient != null)
                {
                    // Проверка существования другого клиента с таким же номером телефона
                    var duplicatePhone = _context.Clients.FirstOrDefault(c => c.Phone == client.Phone && c.Id != id);
                    if (duplicatePhone != null)
                    {
                        _logger.LogWarning("Другой клиент с номером телефона {Phone} уже существует.", client.Phone);
                        return Conflict(new { message = "Другой клиент с таким номером телефона уже существует" });
                    }

                    existingClient.Name = client.Name;
                    existingClient.Phone = client.Phone;
                    // Сохраняем оригинальные значения CreatedAt и CreatedBy
                    client.CreatedAt = existingClient.CreatedAt;
                    client.CreatedBy = existingClient.CreatedBy;

                    _context.SaveChanges();
                    _logger.LogInformation("Клиент с ID {Id} успешно обновлен: {@Client}", id, client);
                    return Ok(new { message = "Клиент успешно обновлен", client });
                }
                else
                {
                    _logger.LogWarning("Клиент с ID {Id} не найден.", id);
                    return NotFound(new { message = "Клиент не найден" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении клиента с ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var client = _context.Clients.FirstOrDefault(c => c.Id == id);
                if (client != null)
                {
                    // Проверяем, есть ли заказы, связанные с этим клиентом
                    var hasOrders = _context.Orders.Any(o => o.CustomerId == id);
                    if (hasOrders)
                    {
                        _logger.LogWarning("Невозможно удалить клиента с ID {Id}, так как у него есть заказы.", id);
                        return BadRequest(new { message = "Невозможно удалить клиента, у которого есть заказы" });
                    }

                    _context.Clients.Remove(client);
                    _context.SaveChanges();
                    _logger.LogInformation("Клиент с ID {Id} успешно удален.", id);
                    return Ok(new { message = "Клиент успешно удален" });
                }
                else
                {
                    _logger.LogWarning("Клиент с ID {Id} не найден для удаления.", id);
                    return NotFound(new { message = "Клиент не найден" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента с ID {Id}.", id);
                var innerMessage = ex.InnerException?.Message;
                return BadRequest(new { message = ex.Message, innerMessage });
            }
        }
    }
}
