using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearInfrastructure.Repositories;

internal class CellRepository : BaseRepository<Cell>, ICellRepository
{
    public CellRepository(ReactorDbContext context) : base(context) { }
}
