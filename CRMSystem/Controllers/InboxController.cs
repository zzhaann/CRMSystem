using CRMSystem.Hubs;
using CRMSystem.Models;
using CRMSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CRMSystem.Controllers
{
    public class InboxController : Controller
    {
        private readonly GreenApiService _greenApiService;
        private readonly MessageStore _messageStore;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<InboxController> _logger;

        public InboxController(GreenApiService greenApiService, MessageStore messageStore, IHubContext<ChatHub> hubContext, ILogger<InboxController> logger)
        {
            _greenApiService = greenApiService;
            _messageStore = messageStore;
            _hubContext = hubContext;
            _logger = logger;
        }

        public IActionResult Index(string chatId = "")
        {
            _logger.LogInformation("Загрузка страницы Index с chatId: {ChatId}", chatId);

            var model = new WhatsAppMessage { ChatId = chatId };
            if (!string.IsNullOrEmpty(chatId))
            {
                ViewBag.Messages = _messageStore.GetMessages(chatId.EndsWith("@c.us") ? chatId : $"{chatId}@c.us");
            }
            else
            {
                ViewBag.Messages = new List<WhatsAppMessage>();
            }
            ViewBag.Contacts = _messageStore.GetChatIds();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] WhatsAppMessage model)
        {
            if (string.IsNullOrEmpty(model.ChatId) || string.IsNullOrEmpty(model.Message))
            {
                _logger.LogWarning("Попытка отправить сообщение с пустым ChatId или Message");
                return Json(new { success = false, error = "ChatId и Message обязательны." });
            }

            var chatId = model.ChatId.EndsWith("@c.us") ? model.ChatId : $"{model.ChatId}@c.us";
            model.ChatId = chatId;
            model.Timestamp = DateTime.UtcNow;
            model.IsIncoming = false;

            _logger.LogInformation("Отправка сообщения в чат {ChatId}: {Message}", chatId, model.Message);

            var (success, idMessage) = await _greenApiService.SendMessageAsync(chatId, model.Message);
            if (success)
            {
                model.IdMessage = idMessage ?? Guid.NewGuid().ToString();
                _messageStore.AddMessage(model);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", model);
                _logger.LogInformation("Сообщение успешно отправлено в чат {ChatId}", chatId);
                return Json(new { success = true });
            }

            _logger.LogError("Ошибка при отправке сообщения в чат {ChatId}", chatId);
            return Json(new { success = false, error = "Ошибка при отправке сообщения." });
        }

        [HttpGet]
        public IActionResult GetMessages(string chatId)
        {
            _logger.LogInformation("Получение сообщений для чата {ChatId}", chatId);

            var messages = _messageStore.GetMessages(chatId.EndsWith("@c.us") ? chatId : $"{chatId}@c.us");
            return Json(messages);
        }
    }
}
