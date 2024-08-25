using Koshel.ApiClient;
using Koshel.Mvc.KoshelApiService;
using Koshel.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class HistoryController : Controller
{
    private readonly IKoshelApiRepositry _apiClient;

    public HistoryController(IKoshelApiRepositry apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _apiClient.GetMessagesAsync();

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
