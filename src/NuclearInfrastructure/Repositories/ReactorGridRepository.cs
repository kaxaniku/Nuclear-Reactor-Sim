using NuclearApp.Interfaces.Repositories;
using NuclearApp.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class ReactorGridRepository : BaseRepository<ReactorGridDto>, IReactorGridRepository
{
    public ReactorGridRepository(ReactorDbContext context) : base(context) { }
}