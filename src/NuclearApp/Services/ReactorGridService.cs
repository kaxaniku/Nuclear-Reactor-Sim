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
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

            return reactorGrid.Cells;
        }

        public async Task<CellDto> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default)
        {
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");
            var cell = reactorGrid.Cells.FirstOrDefault(x => x.Id == cellId);
            if (cell is null)
                throw new InvalidOperationException($"Cell with ID {cellId} not found.");

            return cell;
        }

        public async Task<CellDto> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
        {
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");
            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
            if (cell is null)
                throw new InvalidOperationException($"Cell with coordinates {x}, {y} not found.");

            return cell;
        }

        // Insert a cell into the reactor grid
        public async Task InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
        {
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = new CellDto
            {
                X = command.X,
                Y = command.Y,
                ColumnType = command.NewColumnType,
                Telemetry = TelemetryFactory.CreateDefault(command.NewColumnType)
            };

            reactorGrid.Cells.Add(cell);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Update a cell in the reactor grid
        public async Task UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
        {
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == command.X && c.Y == command.Y);
            if (cell == null)
                throw new InvalidOperationException($"Cell at position ({command.X}, {command.Y}) not found in reactor grid.");

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
            var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
            if (reactorGrid == null)
                throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

            var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
            if (cell == null)
                throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

            reactorGrid.Cells.Remove(cell);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}