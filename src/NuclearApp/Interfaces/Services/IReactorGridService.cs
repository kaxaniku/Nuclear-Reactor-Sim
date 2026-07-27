using NuclearDomain.DTOs;

namespace NuclearApp.Interfaces.Services
{
    public interface IReactorGridService
    {
        Task<List<CellDto>> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default);
        Task InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task DeleteCellAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default);
        Task<CellDto> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default);
        Task<CellDto> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default);
        Task<int> CreateReactorAsync(string name, CancellationToken cancellationToken = default);
        Task<int> GetReactorGridIdByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReactorGridDto>> GetAllReactorGridsAsync(CancellationToken cancellationToken = default);
        Task<ReactorGridDto> GetReactorGridByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}