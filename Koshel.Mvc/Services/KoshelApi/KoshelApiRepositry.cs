using Koshel.ApiClient;
using Koshel.ApiClient.Data;

namespace Koshel.Mvc.KoshelApiService;

public class KoshelApiRepositry : IKoshelApiRepositry
{
    private readonly KoshelApiClient _apiClient;

    public KoshelApiRepositry(IConfiguration configuration)
    {
        var apiUrl = configuration.GetValue<string>("KoshelApi:Url")!;
        _apiClient = new KoshelApiClient(apiUrl);
    }

    public Task<IEnumerable<MessageDto>?> GetMessagesAsync()
    {
        return _apiClient.GetMessagesAsync();
    }

    public Task<MessageDto?> GetMessageAsync(int id)
    {
        return _apiClient.GetMessageAsync(id);
    }

    public Task<MessageDto?> SendMessageAsync(string message)
    {
        return _apiClient.SendMessageAsync(message);
    }
}
