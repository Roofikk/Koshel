using Koshel.ApiClient;
using Koshel.Mvc.KoshelApiService;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class ProducerController : Controller
{
    private readonly IKoshelApiRepositry _apiClient;
    private readonly ILogger<ProducerController> _logger;

    public ProducerController(IKoshelApiRepositry apiClient, ILogger<ProducerController> logger)
    {
        _apiClient = apiClient;
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

        var createdMessage = await _apiClient.SendMessageAsync(message);

        if (createdMessage != null)
        {
            return Ok();
        }

        return BadRequest("Message wasn't created");
    }
}
