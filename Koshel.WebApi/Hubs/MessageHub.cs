using Koshel.DataContext;
using Microsoft.AspNetCore.SignalR;

namespace Koshel.WebApi.Hubs;

public class MessageHub : Hub
{
    public async Task SendMessage(Message message)
    {
        if (Clients is null)
        {
            return;
        }

        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
