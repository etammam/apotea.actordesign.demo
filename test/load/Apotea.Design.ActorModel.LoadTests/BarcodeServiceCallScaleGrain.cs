using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System;
using System.Net.Http;
using NBomberRunner = NBomber.CSharp.NBomberRunner;
using Scenario = NBomber.CSharp.Scenario;

namespace Apotea.Design.ActorModel.LoadTests
{
    public class BarcodeServiceCallScaleGrain
    {
        [Fact]
        public void LoadTest()
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:2001");
            var scenario = Scenario.Create("http_load_test", async context =>
            {
                var step1 = await Step.Run("step_1", context, async () =>
                {
                    var boxId = Random.Shared.Next(60, 120);
                    var request = Http.CreateRequest("GET", $"/api/get-weight/{boxId}");

                    var response = await Http.Send(httpClient, request);

                    return response;
                });

                return Response.Ok();
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1)),
                Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1)),
            Simulation.RampingInject(rate: 0, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
            );

            var result = NBomberRunner
               .RegisterScenarios(scenario)
               .Run();

            var scnStats = result.ScenarioStats.Get("http_load_test");
            var step1Stats = scnStats.StepStats.Get("step_1");

            Assert.True(result.AllBytes > 0);
            Assert.True(result.AllRequestCount > 0);
            Assert.True(result.AllOkCount > 0);
            Assert.True(result.AllFailCount == 0);
            Assert.True(scnStats.Ok.Request.RPS > 0);
            Assert.True(scnStats.Ok.Request.Count > 0);

            Assert.True(scnStats.Ok.Request.Percent == 100);
            Assert.True(scnStats.Fail.Request.Percent == 0);

            Assert.True(scnStats.Ok.Latency.MinMs > 0);
            Assert.True(scnStats.Ok.Latency.MaxMs > 0);

            Assert.True(scnStats.Fail.Request.Count == 0);
            Assert.True(scnStats.Fail.Latency.MinMs == 0);

            Assert.True(step1Stats.Ok.Latency.Percent50 > 0);
            Assert.True(step1Stats.Ok.Latency.Percent75 > 0);
            Assert.True(step1Stats.Ok.Latency.Percent99 > 0);
        }
    }
}