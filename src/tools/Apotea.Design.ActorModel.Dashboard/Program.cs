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
            builder.Host.UseOrleans(siloBuilder =>
            {
                //o.ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory());
                const string redisConnectionString = "localhost";
                const int siloPort = 5001;
                const string siloIpAddress = "127.0.0.1";
                siloBuilder.UseDashboardEmbeddedFiles();
                siloBuilder.UseDashboard();
                siloBuilder.AddActivityPropagation();
                siloBuilder.ConfigureEndpoints(
                    advertisedIP: IPAddress.Parse(siloIpAddress),
                siloPort: siloPort,
                listenOnAnyHostAddress: true,
                    gatewayPort: 30000
                );
                siloBuilder.UseRedisClustering(redisConnectionString);
                siloBuilder.AddRedisGrainStorageAsDefault(rgs =>
                {
                    rgs.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
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
