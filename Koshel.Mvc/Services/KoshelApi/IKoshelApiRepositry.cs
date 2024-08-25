using Koshel.ApiClient.Data;

namespace Koshel.Mvc.KoshelApiService;

public interface IKoshelApiRepositry
{
    public Task<IEnumerable<MessageDto>?> GetMessagesAsync();
    public Task<MessageDto?> GetMessageAsync(int id);
    public Task<MessageDto?> SendMessageAsync(string message);
}