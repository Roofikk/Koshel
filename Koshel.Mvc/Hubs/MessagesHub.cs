using Koshel.ApiClient.Data;
using Microsoft.AspNetCore.SignalR;

namespace Koshel.Mvc.Hubs;

public class MessagesHub : Hub
{
    public async Task SendMessage(MessageDto message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
