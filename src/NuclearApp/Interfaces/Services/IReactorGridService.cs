using NuclearDomain.DTOs;

namespace NuclearApp.Interfaces.Services
{
    public interface IReactorGridService
    {
        Task<List<CellDto>> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default);
        Task InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task DeleteCellAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default);
    }
}