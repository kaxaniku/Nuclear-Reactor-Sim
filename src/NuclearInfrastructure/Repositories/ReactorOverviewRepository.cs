using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class ReactorOverviewRepository : BaseRepository<ReactorOverviewDto>, IReactorOverviewRepository
{
    public ReactorOverviewRepository(ReactorDbContext context) : base(context) { }
}