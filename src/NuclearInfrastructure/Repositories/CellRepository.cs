using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class CellRepository : BaseRepository<CellDto>, ICellRepository
{
    public CellRepository(ReactorDbContext context) : base(context) { }
}
