using Koshel.ApiClient.Data;
using System.Net.Http.Json;

namespace Koshel.ApiClient;

public class KoshelApiClient
{
    private readonly HttpClient _httpClient;

    public KoshelApiClient(string domain = "localhost:5003")
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri($"http://{domain}/api/messages/")
        };
    }

    public async Task<IEnumerable<MessageDto>?> GetMessages(TimeSpan? time = null)
    {
        time ??= TimeSpan.FromMinutes(10);
        var response = await _httpClient.GetAsync($"?time={time:hh\\:mm\\:ss}");

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<MessageDto>>();
    }

    public async Task<MessageDto?> GetMessages(int messageId)
    {
        var response = await _httpClient.GetAsync($"Get/{messageId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MessageDto>();
    }

    public async Task<MessageDto?> SendMessage(string message)
    {
        var response = await _httpClient.PostAsync("", new StringContent($"\"{message}\"", System.Text.Encoding.UTF8, "application/json"));
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MessageDto>();
    }
}
