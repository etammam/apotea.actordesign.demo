using Apotea.Design.ActorModel.Services.IMessages;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Apotea.Design.ActorModel.Services.Scale.Grains
{
    public class ScaleGrain : Grain, IScaleGrain
    {
        private readonly ILogger<ScaleGrain> _logger;

        public ScaleGrain(ILogger<ScaleGrain> logger)
        {
            _logger = logger;
        }

        public Task<int> GetCurrentWeight()
        {
            var sortboxId = this.GetPrimaryKeyLong();
            _logger.LogInformation("getting current weight for sortbox with id {sortboxId}", sortboxId);
            var weight = Random.Shared.Next(100, 800);
            return Task.FromResult(weight);
        }
    }
}
