using Koshel.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class ConsumerController : Controller
{
    private readonly KoshelApiClient _apiClient;
    private readonly ILogger<ConsumerController> _logger;

    public ConsumerController(ILogger<ConsumerController> logger)
    {
        _apiClient = new KoshelApiClient();
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
