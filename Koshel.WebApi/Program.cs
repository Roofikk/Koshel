using Koshel.DataContext;
using Koshel.WebApi.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, lc) =>
{
    lc.ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Logger(innerLc => innerLc
            .Enrich.FromLogContext()
            .Filter.ByExcluding(l => l.Properties.ContainsKey("SourceContext")
                && l.Properties["SourceContext"].ToString().Contains("Jobs")
                && l.Properties["SourceContext"].ToString().Contains("RequestLoggingMiddleware"))
            .WriteTo.File(
                "logs/log-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"))
        .WriteTo.Logger(innerLc => innerLc
            .Enrich.FromLogContext()
            .Filter.ByIncludingOnly(l => l.Properties.ContainsKey("SourceContext") && l.Properties["SourceContext"].ToString().Contains("RequestLoggingMiddleware"))
            .WriteTo.File(
                "logs/requests/log-.log",
                rollingInterval: RollingInterval.Day))
        .WriteTo.Console(outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddKoshelContext(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    builder =>
    {
        builder.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed((host) => true)
            .AllowCredentials();
    }));
builder.Services.AddSignalR();
builder.Services.AddSingleton<MessageHub>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "[IP: {Address}] [{RequestMethod:u3}] [Status: {StatusCode}] [path: {RequestPath}] in {Elapsed:0.0000} ms";

    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Address", httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown");
    };
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.UseCors("CorsPolicy");
app.MapHub<MessageHub>("/messageHub");

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<KoshelContext>();
    await context.InitializeDatabaseAsync();
});

app.Run();
