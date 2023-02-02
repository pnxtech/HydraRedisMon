using HydraRedisMonService;
using Hydra4Net.HostingExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((c) =>
    {
        c.AddJsonFile("appsettings.json");
        c.AddEnvironmentVariables();
    })
    .ConfigureServices((cont, services) =>
    {
        var config = cont.Configuration.GetSection("HydraConfig").GetHydraConfig();
        services
            .AddHydra(config);
        services.AddSingleton<RedisMonitor>();
        services.AddHostedService<ConnectionService>();
    }).Build();

await host.RunAsync();