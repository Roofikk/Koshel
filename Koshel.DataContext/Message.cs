using System.ComponentModel.DataAnnotations.Schema;

namespace Koshel.DataContext;

public class Message
{
    public int MessageId { get; set; }
    [Column(TypeName = "varchar(128)")]
    public string Content { get; set; } = null!;
    public DateTimeOffset SendDate { get; set; } = DateTimeOffset.UtcNow;
}
