using NuclearApp.Interfaces.Repositories;
using NuclearApp.Interfaces.Services;
using NuclearDomain.DTOs;
using NuclearDomain.Factories;

namespace NuclearApp.Services
{
    public class ReactorGridService : IReactorGridService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReactorGridService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        // Read all cells in the reactor grid
        public async Task<List<CellDto>> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

            return reactorGrid.Cells;
        }

        public async Task<CellDto> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");
            var cell = reactorGrid.Cells.FirstOrDefault(x => x.Id == cellId) ?? throw new KeyNotFoundException($"Cell with ID {cellId} not found.");
            return cell;
        }

        public async Task<CellDto> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y) ?? throw new KeyNotFoundException($"Cell with coordinates {x}, {y} not found.");
            return cell;
        }

        public async Task<int> GetReactorGridIdByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var reactorGrid = (await _unitOfWork.ReactorGridRepository.QueryAsync(r => r.Name == name && r.ActivityInfo.IsActive, cancellationToken))
                    .FirstOrDefault() ?? throw new KeyNotFoundException($"Reactor grid with name {name} not found.");
            return reactorGrid.Id;
        }

        public async Task<IEnumerable<ReactorGridDto>> GetAllReactorGridsAsync(CancellationToken cancellationToken = default)
        {
            var reactorsGrids = await _unitOfWork.ReactorGridRepository.QueryAsync(r => r.ActivityInfo.IsActive, cancellationToken);
            return reactorsGrids;
        }

        public async Task<ReactorGridDto> GetReactorGridByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(id, cancellationToken) ??
                throw new KeyNotFoundException($"Reactor grid with ID {id} not found.");
            return reactorGrid;
        }

        // Insert a cell into the reactor grid
        public async Task InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

            if (reactorGrid.Cells.Any(c => c.X == command.X && c.Y == command.Y))
            {
                throw new ArgumentException("Cell already exists at the specified coordinates.");
            }

            var cell = new CellDto
            {
                X = command.X,
                Y = command.Y,
                ColumnType = command.NewColumnType,
                Telemetry = TelemetryFactory.CreateDefault(command.NewColumnType),
                ReactorGridId = reactorGrid.Id
            };

            reactorGrid.Cells.Add(cell);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Update a cell in the reactor grid
        public async Task UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == command.X && c.Y == command.Y) ?? throw new KeyNotFoundException($"Cell at position ({command.X}, {command.Y}) not found in reactor grid.");
            var newColumnType = command.NewColumnType;

            if (cell.ColumnType != newColumnType)
            {
                cell.ColumnType = newColumnType;

                cell.Telemetry = TelemetryFactory.CreateDefault(newColumnType);
            }
            else
            {
                // Optional: If you pass custom telemetry values in command, update properties here.
                // Otherwise, keep existing telemetry readings intact.
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Delete a cell from the reactor grid
        public async Task DeleteCellAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorGridId,
                cancellationToken,
                g => g.Cells
            );

            var reactorGrid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y) ?? throw new KeyNotFoundException($"Cell at position ({x}, {y}) not found in reactor grid.");
            reactorGrid.Cells.Remove(cell);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CreateReactorAsync(string name, CancellationToken cancellationToken = default)
        {
            var reactorGrid = new ReactorGridDto
            {
                Name = name,
                Cells = new List<CellDto>(),
                TotalRows = 0,
                TotalColumns = 0
            };

            await _unitOfWork.ReactorGridRepository.InsertAsync(reactorGrid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return reactorGrid.Id;
        }
    }
}