using NuclearDomain.Entities;

namespace NuclearApp.Interfaces.Repositories;

public interface ICellRepository : IBaseRepository<Cell>
{
    void MarkModified(Cell cell);
    void MarkRangeModified(IEnumerable<Cell> cells);
}
