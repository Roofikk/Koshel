using Koshel.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Koshel.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly KoshelContext _context;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(KoshelContext context, ILogger<MessagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> Get([FromQuery] TimeSpan? time)
    {
        time ??= TimeSpan.FromMinutes(10);
        // SELECT * FROM Messages WHERE CreatedAt > NOW() - INTERVAL '10 MINUTES'
        var query = $"SELECT * FROM \"Messages\" WHERE \"SendDate\" >= NOW() - INTERVAL '{time:hh\\:mm\\:ss}'";
        var messages = await _context.Messages.FromSqlRaw(query).ToListAsync();

        _logger.LogInformation("Got {Count} messages from {Time}", [messages.Count, time]);
        return messages;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Message>> Get(int id)
    {
        var query = "SELECT * FROM \"Messages\" WHERE \"MessageId\"=" + id;
        var message = await _context.Messages.FromSqlRaw(query).FirstOrDefaultAsync();

        if (message == null)
        {
            _logger.LogWarning("Message {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Got message {Id}", id);
        return message;
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Post([FromBody] string message)
    {
        var dateNow = new DateTimeOffset(DateTime.Now);
        var query = "INSERT INTO \"Messages\" (\"Content\", \"SendDate\") VALUES (@Message, @SendDate) RETURNING \"MessageId\"";

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("Message", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = message });
            command.Parameters.Add(new NpgsqlParameter("SendDate", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = dateNow });
            await _context.Database.OpenConnectionAsync();
            var messageId = await command.ExecuteScalarAsync();
            await _context.Database.CloseConnectionAsync();

            if (messageId == null)
            {
                _logger.LogWarning("Failed to post message {Message}", message);
                return BadRequest();
            }

            return CreatedAtAction(nameof(Get), new { id = messageId }, new Message { MessageId = (int)messageId, Content = message, SendDate = dateNow });
        }
    }
}

class TimeSpanConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            var regex = new Regex(@"^(?<hours>\d+):(?<minutes>\d+):(?<seconds>\d+)$");

            if (regex.IsMatch(s))
            {
                var match = regex.Match(s);
                var hours = int.Parse(match.Groups["hours"].Value);
                var minutes = int.Parse(match.Groups["minutes"].Value);
                var seconds = int.Parse(match.Groups["seconds"].Value);

                if (hours > 23 || minutes > 59 || seconds > 59)
                {
                    return null;
                }

                return new TimeSpan(hours, minutes, seconds);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
