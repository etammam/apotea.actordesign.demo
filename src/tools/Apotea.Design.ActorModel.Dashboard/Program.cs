using Apotea.Design.ActorModel.Services.ServicesDefault;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Versions.Compatibility;
using Orleans.Versions.Selector;
using StackExchange.Redis;
using System.Net;

namespace Apotea.Design.ActorModel.Dashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<ClusterSiloConfiguration>(builder.Configuration.GetSection(nameof(ClusterSiloConfiguration)));

            builder.Host.UseOrleans(siloBuilder =>
            {
                var serviceProvider = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider;
                var options = serviceProvider.GetRequiredService<IOptions<ClusterSiloConfiguration>>();

                ArgumentNullException.ThrowIfNull(options.Value.IpAddress, "orleans cluster configuration IpAddress required");
                ArgumentNullException.ThrowIfNull(options.Value.Port, "orleans cluster configuration port required");
                ArgumentNullException.ThrowIfNull(options.Value.RedisConnectionString, "orleans cluster configuration redis connectionString required");

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

                siloBuilder.UseDashboardEmbeddedFiles();
                siloBuilder.UseDashboard();
                siloBuilder.AddActivityPropagation();

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
            var app = builder.Build();
            app.Map("/dashboard", x => x.UseOrleansDashboard());
            app.Run();
        }
    }
}
