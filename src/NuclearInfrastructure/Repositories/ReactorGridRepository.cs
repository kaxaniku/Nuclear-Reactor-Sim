using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearInfrastructure.Repositories;

internal class ReactorGridRepository : BaseRepository<ReactorGrid>, IReactorGridRepository
{
    public ReactorGridRepository(ReactorDbContext context) : base(context) { }
}