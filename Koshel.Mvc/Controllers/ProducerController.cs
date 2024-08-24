using Koshel.ApiClient;
using Koshel.Mvc.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class ProducerController : Controller
{
    private readonly KoshelApiClient _apiClient;
    private readonly MessagesHub _hub;
    private readonly ILogger<ProducerController> _logger;

    public ProducerController(MessagesHub hub, ILogger<ProducerController> logger)
    {
        _apiClient = new KoshelApiClient();
        _hub = hub;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] string message)
    {
        if (!ModelState.IsValid)
        {
            return View("Error");
        }

        var createdMessage = await _apiClient.SendMessage(message);

        if (createdMessage != null)
        {
            await _hub.SendMessage(createdMessage);
            return View("Success");
        }

        return View("Error");
    }
}
