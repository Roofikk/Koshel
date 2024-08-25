using Koshel.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class ProducerController : Controller
{
    private readonly KoshelApiClient _apiClient;
    private readonly ILogger<ProducerController> _logger;

    public ProducerController(ILogger<ProducerController> logger)
    {
        _apiClient = new KoshelApiClient();
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
            return BadRequest(ModelState);
        }

        var createdMessage = await _apiClient.SendMessage(message);

        if (createdMessage != null)
        {
            return Ok();
        }

        return BadRequest("Message wasn't created");
    }
}
