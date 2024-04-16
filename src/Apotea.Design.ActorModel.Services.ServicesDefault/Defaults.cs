using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
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
            ArgumentNullException.ThrowIfNull(options.Value.IpAddressd, "orleans cluster configuration IpAddress required");
            ArgumentNullException.ThrowIfNull(options.Value.Port, "orleans cluster configuration port required");
            ArgumentNullException.ThrowIfNull(options.Value.RedisConnectionString, "orleans cluster configuration redis connectionString required");

            services.AddOrleans(siloBuilder =>
            {
                siloBuilder.AddActivityPropagation();
                siloBuilder.ConfigureEndpoints(
                    advertisedIP: IPAddress.Parse(options.Value.IpAddressd),
                    siloPort: options.Value.Port,
                    listenOnAnyHostAddress: true,
                    gatewayPort: 30000
                );
                siloBuilder.UseRedisClustering(options.Value.RedisConnectionString);
                siloBuilder.AddRedisGrainStorage(options.Value.RedisConnectionString);
                siloBuilder.AddReminders();
            });
        }
    }
}
