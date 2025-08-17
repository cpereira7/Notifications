using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monitor.Database;
using Monitor.Model;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        var payloadTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(NotificationPayload)) &&
                           type.GetCustomAttribute<PostgresChannelAttribute>() != null);

        foreach (var type in payloadTypes)
        {
            var listenerType = typeof(DatabaseListener<>).MakeGenericType(type);
            services.AddSingleton(listenerType);

            var hostedType = typeof(ListenerHostedService<>).MakeGenericType(type);
            services.AddSingleton(typeof(IHostedService), hostedType);
        }
    })
    .Build();

await host.RunAsync();