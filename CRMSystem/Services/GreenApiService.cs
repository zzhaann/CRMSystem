using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using CRMSystem.Hubs;
using CRMSystem.Models;

namespace CRMSystem.Services
{
    public class GreenApiService
    {
        private readonly string _apiUrl;
        private readonly string _idInstance;
        private readonly string _apiTokenInstance;
        private readonly HttpClient _httpClient;
        private readonly MessageStore _messageStore;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<GreenApiService> _logger;

        public GreenApiService(IConfiguration configuration, MessageStore messageStore, IHubContext<ChatHub> hubContext, ILogger<GreenApiService> logger)
        {
            _apiUrl = configuration["GreenApi:ApiUrl"];
            _idInstance = configuration["GreenApi:IdInstance"];
            _apiTokenInstance = configuration["GreenApi:ApiTokenInstance"];
            _httpClient = new HttpClient();
            _messageStore = messageStore;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<(bool Success, string? IdMessage)> SendMessageAsync(string chatId, string message)
        {
            var url = $"{_apiUrl}/waInstance{_idInstance}/sendMessage/{_apiTokenInstance}";
            var requestBody = new { chatId, message };
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Сообщение отправлено: {ResponseBody}", responseBody);

                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("idMessage", out var idMessageElement))
                    {
                        return (true, idMessageElement.GetString());
                    }

                    return (true, null);
                }

                _logger.LogWarning("Ошибка отправки: {StatusCode}", response.StatusCode);
                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения");
                return (false, null);
            }
        }

        public async Task<List<WhatsAppMessage>> ReceiveNotificationsAsync()
        {
            var url = $"{_apiUrl}/waInstance{_idInstance}/receiveNotification/{_apiTokenInstance}";
            var messages = new List<WhatsAppMessage>();
            var startTime = DateTime.UtcNow;

            try
            {
                while (true)
                {
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Ошибка получения уведомлений: {StatusCode}", response.StatusCode);
                        break;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "null")
                    {
                        break;
                    }

                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("body", out var body) ||
                        !body.TryGetProperty("typeWebhook", out var typeWebhook) ||
                        typeWebhook.GetString() != "incomingMessageReceived")
                    {
                        await DeleteNotification(root.GetProperty("receiptId").GetInt32());
                        continue;
                    }

                    var messageData = body.GetProperty("messageData");
                    if (messageData.GetProperty("typeMessage").GetString() != "textMessage")
                    {
                        await DeleteNotification(root.GetProperty("receiptId").GetInt32());
                        continue;
                    }

                    var incomingMessage = new WhatsAppMessage
                    {
                        ChatId = body.GetProperty("senderData").GetProperty("chatId").GetString(),
                        Message = messageData.GetProperty("textMessageData").GetProperty("textMessage").GetString(),
                        Timestamp = body.TryGetProperty("timestamp", out var timestamp)
                            ? DateTimeOffset.FromUnixTimeSeconds(timestamp.GetInt64()).DateTime
                            : DateTime.UtcNow,
                        IsIncoming = true,
                        IdMessage = root.TryGetProperty("idMessage", out var idMessage)
                            ? idMessage.GetString()
                            : Guid.NewGuid().ToString()
                    };

                    messages.Add(incomingMessage);
                    await DeleteNotification(root.GetProperty("receiptId").GetInt32());
                }

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation("Получено {MessageCount} новых входящих сообщений через ReceiveNotification за {Duration:F2} секунд", messages.Count, duration);
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения уведомлений");
                return messages;
            }
        }

        private async Task DeleteNotification(int receiptId)
        {
            var deleteUrl = $"{_apiUrl}/waInstance{_idInstance}/deleteNotification/{_apiTokenInstance}/{receiptId}";
            await _httpClient.DeleteAsync(deleteUrl);
        }

        public async Task ProcessWebhookAsync(JsonElement webhook)
        {
            try
            {
                _logger.LogInformation("Вебхук получен: {Webhook}", webhook.ToString());

                if (webhook.TryGetProperty("typeWebhook", out var typeWebhook) &&
                    typeWebhook.GetString() == "incomingMessageReceived")
                {
                    if (!webhook.TryGetProperty("body", out var body))
                    {
                        body = webhook;
                    }

                    var messageData = body.GetProperty("messageData");
                    if (messageData.GetProperty("typeMessage").GetString() != "textMessage")
                    {
                        return;
                    }

                    string idMessageValue = webhook.TryGetProperty("idMessage", out var idMessage)
                        ? idMessage.GetString()
                        : Guid.NewGuid().ToString();

                    var incomingMessage = new WhatsAppMessage
                    {
                        ChatId = body.GetProperty("senderData").GetProperty("chatId").GetString(),
                        Message = messageData.GetProperty("textMessageData").GetProperty("textMessage").GetString(),
                        Timestamp = body.TryGetProperty("timestamp", out var timestamp)
                            ? DateTimeOffset.FromUnixTimeSeconds(timestamp.GetInt64()).DateTime
                            : DateTime.UtcNow,
                        IsIncoming = true,
                        IdMessage = idMessageValue
                    };

                    _logger.LogInformation("Извлечённый IdMessage: {IdMessage}", idMessageValue);

                    _messageStore.AddMessage(incomingMessage);
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", incomingMessage);

                    _logger.LogInformation("Входящее сообщение обработано: {Message}", incomingMessage.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки вебхука");
            }
        }
    }
}
