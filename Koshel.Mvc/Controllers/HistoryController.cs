using Koshel.ApiClient;
using Koshel.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class HistoryController : Controller
{
    private readonly KoshelApiClient _apiClient;

    public HistoryController()
    {
        _apiClient = new KoshelApiClient();
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _apiClient.GetMessages();

        if (messages != null)
        {
            return View(messages.Select(x => new MessageModel()
            {
                MessageId = x.MessageId,
                Content = x.Content,
                SendDate = x.SendDate
            }));
        }

        return View("Not found messages");
    }
}
