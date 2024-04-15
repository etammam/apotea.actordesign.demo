using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using System.Net;

namespace Apotea.Design.ActorModel.Services.ServicesDefault
{
    public static class Defaults
    {
        public static void AddOrleansCluster(this IHostBuilder builder, IOptions<ClusterSiloConfiguration> options)
        {
            var configurations = options.Value;

            builder.UseOrleans(siloBuilder =>
            {
                siloBuilder.AddActivityPropagation();
                siloBuilder.ConfigureEndpoints(
                    advertisedIP: IPAddress.Parse(options.Value.IpAddressd),
                    siloPort: options.Value.Port,
                    listenOnAnyHostAddress: true,
                    gatewayPort: 30000
                );
            });
        }
    }
}
