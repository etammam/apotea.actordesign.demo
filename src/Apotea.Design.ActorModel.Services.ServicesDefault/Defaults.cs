using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
            var options = services.BuildServiceProvider().CreateScope()
                .ServiceProvider.GetRequiredService<IOptions<ClusterSiloConfiguration>>();
            ArgumentNullException.ThrowIfNull(options.Value.IpAddress, "orleans cluster configuration IpAddress required");
            ArgumentNullException.ThrowIfNull(options.Value.Port, "orleans cluster configuration port required");
            ArgumentNullException.ThrowIfNull(options.Value.RedisConnectionString, "orleans cluster configuration redis connectionString required");

            services.AddOrleans(siloBuilder =>
            {
                siloBuilder.AddActivityPropagation();
                siloBuilder.ConfigureEndpoints(
                    advertisedIP: IPAddress.Parse(options.Value.IpAddress),
                    siloPort: options.Value.Port,
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

                siloBuilder.AddReminders();
                siloBuilder.UseInMemoryReminderService();
            });
        }
    }
}
