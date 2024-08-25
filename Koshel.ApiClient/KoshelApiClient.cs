using Koshel.ApiClient.Data;
using System.Net.Http.Json;

namespace Koshel.ApiClient;

public class KoshelApiClient
{
    private readonly HttpClient _httpClient;

    public KoshelApiClient(string domain = "http://localhost:5003")
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri($"{domain}/api/messages/")
        };
    }

    public async Task<IEnumerable<MessageDto>?> GetMessagesAsync(DateTimeOffset? beginDate = null, DateTimeOffset? endDate = null)
    {
        endDate ??= new DateTimeOffset(DateTime.Now);
        beginDate ??= endDate.Value.Subtract(TimeSpan.FromMinutes(10));
        var response = await _httpClient.GetAsync($"?beginDate={beginDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<MessageDto>>();
    }

    public async Task<MessageDto?> GetMessageAsync(int messageId)
    {
        var response = await _httpClient.GetAsync($"{messageId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MessageDto>();
    }

    public async Task<MessageDto?> SendMessageAsync(string message)
    {
        var response = await _httpClient.PostAsync("", new StringContent($"\"{message}\"", System.Text.Encoding.UTF8, "application/json"));
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MessageDto>();
    }
}
