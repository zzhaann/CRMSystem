using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CRMSystem.Models;

namespace CRMSystem.Services
{
    public class MessageStore
    {
        private readonly ConcurrentDictionary<string, List<WhatsAppMessage>> _messages = new();
        private readonly ConcurrentBag<string> _chatIds = new();

        public void AddMessage(WhatsAppMessage message)
        {
            var chatId = message.ChatId;
            _messages.AddOrUpdate(
                chatId,
                new List<WhatsAppMessage> { message },
                (key, oldValue) => { oldValue.Add(message); return oldValue; });

            if (!_chatIds.Contains(chatId))
            {
                _chatIds.Add(chatId);
            }

            Console.WriteLine($"Сообщение добавлено в MessageStore: ChatId={chatId}, Message={message.Message}, Timestamp={message.Timestamp}, IsIncoming={message.IsIncoming}, IdMessage={message.IdMessage ?? "null"}");
        }

        public List<WhatsAppMessage> GetMessages(string chatId)
        {
            if (_messages.TryGetValue(chatId, out var messages))
            {
                Console.WriteLine($"Получено {messages.Count} сообщений для ChatId={chatId}");
                return messages.OrderBy(m => m.Timestamp).ToList();
            }

            Console.WriteLine($"Сообщений для ChatId={chatId} не найдено");
            return new List<WhatsAppMessage>();
        }

        public List<string> GetChatIds()
        {
            var chatIds = _chatIds.OrderBy(id => id).ToList();
            Console.WriteLine($"Получено {chatIds.Count} уникальных ChatId");
            return chatIds;
        }

        public void AddContact(string chatId)
        {
            if (!_chatIds.Contains(chatId))
            {
                _chatIds.Add(chatId);
                Console.WriteLine($"Новый контакт добавлен: ChatId={chatId}");
            }
        }
    }
}