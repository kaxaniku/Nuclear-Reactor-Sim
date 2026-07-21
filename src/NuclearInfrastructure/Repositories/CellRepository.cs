using NuclearApp.Interfaces.Repositories;

namespace NuclearInfrastructure.Repositories;

internal class CellRepository : BaseRepository<NuclearApp.DTOs.CellDto>, ICellRepository
{
    public CellRepository(ReactorDbContext context) : base(context) { }
}
