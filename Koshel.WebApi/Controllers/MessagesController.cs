using Koshel.DataContext;
using Koshel.WebApi.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Koshel.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly KoshelContext _context;
    private readonly MessageHub _messageHub;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        KoshelContext context,
        MessageHub messageHub,
        ILogger<MessagesController> logger)
    {
        _context = context;
        _messageHub = messageHub;
        _logger = logger;
    }

    /// <summary>
    /// Получает список сообщений из базы данных на основе указанных дат начала и окончания.
    /// </summary>
    /// <param name="beginDate">Дата начала сообщений. Если не указана, по умолчанию устанавливается текущая дата и время минус 10 минут.</param>
    /// <param name="endDate">Дата окончания сообщений. Если не указана, по умолчанию устанавливается текущая дата и время.</param>
    /// <returns>Список сообщений в указанном диапазоне дат.</returns>
    /// <response code="200">Возвращает список сообщений.</response>
    /// <response code="400">Возвращает ошибку "Неверный запрос", если дата начала больше даты окончания.</response>
    [ProducesResponseType(typeof(IEnumerable<Message>), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> Get([FromQuery] DateTimeOffset? beginDate, [FromQuery] DateTimeOffset? endDate)
    {
        endDate ??= new DateTimeOffset(DateTime.Now);
        beginDate ??= endDate.Value.Subtract(TimeSpan.FromMinutes(10.0));

        if (beginDate > endDate)
        {
            _logger.LogWarning("Begin date {BeginDate} is greater than end date {EndDate}", [beginDate, endDate]);
            return BadRequest("Begin date is greater than end date");
        }

        var query = $"SELECT * FROM \"Messages\" WHERE \"SendDate\" BETWEEN @BeginDate AND @EndDate";

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("BeginDate", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = beginDate });
            command.Parameters.Add(new NpgsqlParameter("EndDate", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = endDate });
            await _context.Database.OpenConnectionAsync();
            var messages = await command.ExecuteReaderAsync();

            var messagesList = new List<Message>();
            while (await messages.ReadAsync())
            {
                messagesList.Add(new Message
                {
                    MessageId = messages.GetInt32(0),
                    Content = messages.GetString(1),
                    SendDate = messages.GetDateTime(2),
                });
            }

            await _context.Database.CloseConnectionAsync();

            _logger.LogInformation("Got messages from {BeginDate} to {EndDate}", [beginDate, endDate]);
            return messagesList;
        }

    }

    /// <summary>
    /// Получает сообщение по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сообщения для получения.</param>
    /// <returns>Полученное сообщение, или NotFound, если сообщение не найдено.</returns>
    /// <response code="200">Возвращает полученное сообщение, если оно было успешно найдено.</response>
    /// <response code="404">Возвращает NotFound, если сообщение с указанным идентификатором не было найдено.</response>
    [ProducesResponseType(typeof(Message), 200)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Создает новую запись в таблице "Messages" базы данных и отправляет ее в hub сообщений.
    /// </summary>
    /// <param name="message">Содержимое сообщения для создания.</param>
    /// <returns>Созданое сообщение, если оно было успешно создано, или BadRequest, если сообщение не удалось отправить.</returns>
    /// <response code="201">Сообщение было успешно создано и отправлено.</response>
    /// <response code="400">Сообщение не удалсь создать.</response>
    [ProducesResponseType(typeof(Message), 201)]
    [ProducesResponseType(400)]
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
            var messageIdObject = await command.ExecuteScalarAsync();
            await _context.Database.CloseConnectionAsync();

            if (messageIdObject == null || messageIdObject is not int messageId)
            {
                _logger.LogWarning("Failed to send message {Message}", message);
                return BadRequest();
            }

            var createdMessage = new Message
            {
                MessageId = messageId,
                Content = message,
                SendDate = dateNow
            };

            await _messageHub.SendMessage(createdMessage);
            return CreatedAtAction(nameof(Get), new { id = messageId }, createdMessage);
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
