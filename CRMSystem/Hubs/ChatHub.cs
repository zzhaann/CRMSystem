using CRMSystem.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace CRMSystem.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(WhatsAppMessage message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
