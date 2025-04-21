using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class WhatsAppMessage
    {
        public int Id { get; set; }
        public string ChatId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsIncoming { get; set; }
        public string? IdMessage { get; set; }
    }
}
