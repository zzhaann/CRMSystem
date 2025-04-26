using CRMSystem.Hubs;
using CRMSystem.Models;
using CRMSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRMSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly MessageStore _messageStore;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(MessageStore messageStore, IHubContext<ChatHub> hubContext, ILogger<WhatsAppWebhookController> logger)
        {
            _messageStore = messageStore;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement data)
        {
            _logger.LogInformation("Вебхук получен: {WebhookData}", data.ToString());

            try
            {
                if (!data.TryGetProperty("typeWebhook", out var typeWebhookElement))
                {
                    _logger.LogWarning("Ошибка: поле 'typeWebhook' отсутствует в JSON");
                    return BadRequest("Поле 'typeWebhook' отсутствует");
                }

                var typeWebhook = typeWebhookElement.GetString();
                if (typeWebhook != "incomingMessageReceived")
                {
                    _logger.LogInformation("Вебхук не является входящим сообщением: typeWebhook={TypeWebhook}", typeWebhook);
                    return Ok();
                }

                if (!data.TryGetProperty("messageData", out var messageData) ||
                    !messageData.TryGetProperty("typeMessage", out var typeMessageElement))
                {
                    _logger.LogWarning("Ошибка: 'messageData' или 'typeMessage' отсутствует");
                    return BadRequest("Некорректный формат messageData");
                }

                if (typeMessageElement.GetString() != "textMessage")
                {
                    _logger.LogInformation("Сообщение не текстовое, игнорируем");
                    return Ok();
                }

                if (!data.TryGetProperty("senderData", out var senderData) ||
                    !senderData.TryGetProperty("chatId", out var chatIdElement) ||
                    !messageData.TryGetProperty("textMessageData", out var textMessageData) ||
                    !textMessageData.TryGetProperty("textMessage", out var textMessageElement))
                {
                    _logger.LogWarning("Ошибка: отсутствуют необходимые поля (chatId или textMessage)");
                    return BadRequest("Отсутствуют необходимые поля");
                }

                var chatId = chatIdElement.GetString();
                var message = textMessageElement.GetString();

                string idMessage = data.TryGetProperty("idMessage", out var idMessageElement)
                    ? idMessageElement.GetString()
                    : Guid.NewGuid().ToString();

                var incomingMessage = new WhatsAppMessage
                {
                    ChatId = chatId,
                    Message = message,
                    Timestamp = data.TryGetProperty("timestamp", out var timestampElement)
                        ? DateTimeOffset.FromUnixTimeSeconds(timestampElement.GetInt64()).UtcDateTime
                        : DateTime.UtcNow,
                    IsIncoming = true,
                    IdMessage = idMessage
                };

                _messageStore.AddMessage(incomingMessage);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", incomingMessage);
                _logger.LogInformation("Входящее сообщение обработано: {Message}", message);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка вебхука");
                return BadRequest(ex.Message);
            }
        }
    }
}
