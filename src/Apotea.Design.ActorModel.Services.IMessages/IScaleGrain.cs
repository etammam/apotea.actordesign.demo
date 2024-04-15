using Orleans;
using System.Threading.Tasks;

namespace Apotea.Design.ActorModel.Services.IMessages
{
    public interface IScaleGrain : IGrainWithIntegerKey
    {
        Task<int> GetCurrentWeight();
    }
}
