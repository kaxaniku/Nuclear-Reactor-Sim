using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class ScramReactorCommandRepository : BaseRepository<ScramReactorCommandDto>, IScramReactorCommandRepository
{
    public ScramReactorCommandRepository(ReactorDbContext context) : base(context) { }
}