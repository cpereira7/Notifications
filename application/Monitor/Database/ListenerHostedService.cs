using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monitor.Logging;
using Monitor.Model;

namespace Monitor.Database;

internal class ListenerHostedService<TModel> : BackgroundService
    where TModel : NotificationPayload
{
    private readonly DatabaseListener<TModel> _listener;
    private readonly ILogger<ListenerHostedService<TModel>> _logger;

    public ListenerHostedService(DatabaseListener<TModel> listener, ILogger<ListenerHostedService<TModel>> logger)
    {
        _listener = listener;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogStartedListener(typeof(TModel).Name);
        
        await _listener.ListenAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _listener.DisposeAsync().ConfigureAwait(false);
    }
}