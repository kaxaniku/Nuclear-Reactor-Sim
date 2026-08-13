using NuclearDomain.Entities;

namespace NuclearApp.Interfaces.Services
{
    public interface IReactorPhysicsEngine
    {
        void ProcessPhysicsTick(ReactorGrid grid, double deltaTimeSeconds);
    }
}