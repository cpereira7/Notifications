using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monitor;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DatabaseListener<EmergencyPayload>>();
    })
    .Build();

var databaseListener = host.Services.GetService<DatabaseListener<EmergencyPayload>>();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) => cts.Cancel();

await databaseListener!.StartListeningAsync(cts.Token);