using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MultiMinesweeper.Hub
{
    [Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("ChatHub hub connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("ChatHub hub disconnected");
            return base.OnDisconnectedAsync(exception);
        }
        
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Message Received by: {user}");
            await this.Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }
    }
}