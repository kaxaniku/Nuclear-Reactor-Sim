using NuclearApp.DTOs;
using NuclearDomain.Entities;

namespace NuclearApp.Interfaces.Services
{
    public interface IReactorGridService
    {
        Task<List<Cell>> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default);
        Task InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default);
        Task DeleteCellAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default);
        Task<Cell> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default);
        Task<Cell> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default);
        Task<int> CreateReactorAsync(string name, CancellationToken cancellationToken = default);
        Task<int> GetReactorGridIdByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReactorGrid>> GetAllReactorGridsAsync(CancellationToken cancellationToken = default);
        Task<ReactorGrid> GetReactorGridByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}