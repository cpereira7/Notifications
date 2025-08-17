using System.Reflection;
using Microsoft.AspNetCore.HttpLogging;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
    var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
    
    var config = new ConfigurationOptions
    {
        AbortOnConnectFail = false,
        EndPoints =
        {
            { redisHost, int.Parse(redisPort) },
        },
        KeepAlive = 180,
        Password = "",
        LoggerFactory = sp.GetService<ILoggerFactory>()
    };
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode | 
                            HttpLoggingFields.Duration;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
    options.CombineLogs = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseHttpLogging();

app.MapGet("/events", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();

    var server = redis.GetServer(redis.GetEndPoints()[0]);
    var keys = server.Keys(pattern: "event:*");

    var events = new List<object>();

    foreach (var key in keys)
    {
        var hash = await db.HashGetAllAsync(key);
        var eventObj = hash.ToDictionary(
            entry => entry.Name.ToString(),
            entry => entry.Value.ToString());

        eventObj["id"] = key.ToString();
        events.Add(eventObj);
    }

    return Results.Ok(events);
});

app.MapGet("/", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
});

await app.RunAsync();