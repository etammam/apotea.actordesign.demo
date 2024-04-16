using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Versions.Compatibility;
using Orleans.Versions.Selector;
using StackExchange.Redis;
using System;
using System.Net;

namespace Apotea.Design.ActorModel.Services.ServicesDefault
{
    public static class Defaults
    {
        public static void AddOrleansClusterOptions(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<ClusterSiloConfiguration>(configuration.GetSection(nameof(ClusterSiloConfiguration)));
        }

        public static void AddOrleansClusterSetup(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider().CreateScope()
                .ServiceProvider;
            var options = serviceProvider.GetRequiredService<IOptions<ClusterSiloConfiguration>>();

            ArgumentNullException.ThrowIfNull(options.Value.IpAddress, "orleans cluster configuration IpAddress required");
            ArgumentNullException.ThrowIfNull(options.Value.Port, "orleans cluster configuration port required");
            ArgumentNullException.ThrowIfNull(options.Value.RedisConnectionString, "orleans cluster configuration redis connectionString required");

            services.AddOrleans(siloBuilder =>
            {
                siloBuilder.AddActivityPropagation();

                var isPortAvailable = NetworkScanner.IsPortOpen(options.Value.IpAddress, options.Value.Port, TimeSpan.FromSeconds(2));
                var newPort = NetworkScanner.GetPort();
                if (!isPortAvailable)
                    Console.WriteLine("port {0}, is not available switching to another port: {1}", options.Value.Port, newPort);

                siloBuilder.ConfigureEndpoints(
                    advertisedIP: IPAddress.Parse(options.Value.IpAddress),
                    siloPort: isPortAvailable ? options.Value.Port : newPort,
                    listenOnAnyHostAddress: true,
                    gatewayPort: 30000
                );

                siloBuilder.UseRedisClustering(options.Value.RedisConnectionString);

                siloBuilder.AddRedisGrainStorageAsDefault(rgs =>
                {
                    rgs.ConfigurationOptions = ConfigurationOptions.Parse(options.Value.RedisConnectionString);
                });

                siloBuilder.Configure<GrainVersioningOptions>(options =>
                {
                    options.DefaultCompatibilityStrategy = nameof(BackwardCompatible);
                    options.DefaultVersionSelectorStrategy = nameof(MinimumVersion);
                });

                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "demo-cluster";
                    options.ServiceId = "demo-service";
                });

                siloBuilder.UseDashboard(o => o.HostSelf = false);
                siloBuilder.AddReminders();
                siloBuilder.UseInMemoryReminderService();
            });
        }
    }
}
