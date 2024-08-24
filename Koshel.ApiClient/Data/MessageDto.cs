namespace Koshel.ApiClient.Data;

public class MessageDto
{
    public int MessageId { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset SendDate { get; set; }
}
