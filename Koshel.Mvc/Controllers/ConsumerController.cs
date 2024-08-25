using Koshel.ApiClient;
using Koshel.Mvc.KoshelApiService;
using Microsoft.AspNetCore.Mvc;

namespace Koshel.Mvc.Controllers;

public class ConsumerController : Controller
{
    private readonly IKoshelApiRepositry _apiClient;
    private readonly ILogger<ConsumerController> _logger;

    public ConsumerController(IKoshelApiRepositry apiClient, ILogger<ConsumerController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
