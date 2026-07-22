using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class ReactorGridRepository : BaseRepository<ReactorGridDto>, IReactorGridRepository
{
    public ReactorGridRepository(ReactorDbContext context) : base(context) { }
}