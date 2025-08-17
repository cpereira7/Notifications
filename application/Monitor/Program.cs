using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monitor.Database;
using Monitor.Model;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
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