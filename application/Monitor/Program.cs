using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monitor;
using Monitor.Database;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DatabaseListener<EmergencyPayload>>();
        services.AddHostedService<ListenerHostedService<EmergencyPayload>>();
    })
    .Build();

await host.RunAsync();