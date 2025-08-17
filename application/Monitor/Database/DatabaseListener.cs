using System.Data;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monitor.Logging;
using Monitor.Model;
using Newtonsoft.Json;
using Npgsql;

namespace Monitor.Database;

internal class DatabaseListener<TModel> : IAsyncDisposable
    where TModel : NotificationPayload
{
    private readonly ILogger<DatabaseListener<TModel>> _logger;
    private readonly NpgsqlConnection? _connection;
    private readonly string _channelName;

    public DatabaseListener(IConfiguration configuration, ILogger<DatabaseListener<TModel>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        _connection = new NpgsqlConnection(connectionString);
        _connection.Notification += OnDatabaseNotification;

        _channelName = ResolveChannelName(typeof(TModel));
    }

    private static string ResolveChannelName(Type modelType)
    {
        var attribute = modelType.GetCustomAttribute<PostgresChannelAttribute>();
        return attribute?.ChannelName 
               ?? throw new InvalidOperationException($"The type {modelType.Name} must have a PostgresChannel attribute defined.");
    }

    private void OnDatabaseNotification(object sender, NpgsqlNotificationEventArgs e)
    {
        try
        {
            var payload = JsonConvert.DeserializeObject<TModel>(e.Payload)
                          ?? throw new JsonSerializationException($"Invalid JSON data format for type {typeof(TModel).Name}");

            _logger.LogNotification(payload.Data);
        }
        catch (Exception ex)
        {
            _logger.LogNonConformNotification(ex, e.Payload);
        }
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("Database connection is not initialized.");
        }
        
        await _connection.OpenAsync(cancellationToken);

        var safeChannel = "\"" + _channelName.Replace("\"", "\"\"") + "\"";
        await using var command = new NpgsqlCommand($"LISTEN {safeChannel}", _connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await _connection!.WaitAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
    }
}