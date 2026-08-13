using Microsoft.EntityFrameworkCore;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearInfrastructure.Repositories;

internal class CellRepository : BaseRepository<Cell>, ICellRepository
{
    private readonly ReactorDbContext _context;

    public CellRepository(ReactorDbContext context) : base(context)
    {
        _context = context;
    }

    public void MarkModified(Cell cell)
    {
        _context.Entry(cell).State = EntityState.Modified;
    }

    public void MarkRangeModified(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            var entry = _context.Entry(cell);
            entry.State = EntityState.Modified;

            entry.Property(c => c.Telemetry).IsModified = true;
        }
    }
}
