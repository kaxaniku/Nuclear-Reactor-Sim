using System.Text;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearApp.Interfaces.Services;
using NuclearDomain.Entities;
using NuclearDomain.Factories;

namespace NuclearApp.Services;

public class ReactorGridService : IReactorGridService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReactorGridService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    // Read all cells in the reactor grid
    public async Task<List<Cell>> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default)
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

    public async Task<Cell> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default)
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

    public async Task<Cell> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<ReactorGrid>> GetAllReactorGridsAsync(CancellationToken cancellationToken = default)
    {
        var reactorsGrids = await _unitOfWork.ReactorGridRepository.QueryAsync(r => r.ActivityInfo.IsActive, cancellationToken);
        return reactorsGrids;
    }

    public async Task<ReactorGrid> GetReactorGridByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException($"Reactor grid with ID {id} not found.");
        return reactorGrid;
    }

    // Insert a cell into the reactor grid
    public async Task<Cell> InsertCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
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

        var cell = new Cell
        {
            X = command.X,
            Y = command.Y,
            ColumnType = command.NewColumnType,
            Telemetry = TelemetryFactory.CreateDefault(command.NewColumnType),
            ReactorGridId = reactorGrid.Id
        };

        reactorGrid.Cells.Add(cell);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cell;
    }

    // Update a cell in the reactor grid
    public async Task<Cell> UpdateCellAsync(int reactorGridId, ConfigureCellCommandDto command, CancellationToken cancellationToken = default)
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
        return cell;
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

    public async Task<ReactorGrid> CreateReactorAsync(string name, CancellationToken cancellationToken = default)
    {
        var reactorGrid = new ReactorGrid
        {
            Name = name,
            Cells = new List<Cell>(),
            TotalRows = 0,
            TotalColumns = 0
        };

        await _unitOfWork.ReactorGridRepository.InsertAsync(reactorGrid, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reactorGrid;
    }

    public async Task DeleteReactorAsync(int id, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException($"Reactor grid with ID {id} not found.");
        _unitOfWork.ReactorGridRepository.Delete(reactorGrid);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> Get2DGridDesignAsync(int reactorGridId, CancellationToken cancellationToken = default)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == reactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

        int minX = reactorGrid.Cells.Min(c => c.X);
        int maxX = reactorGrid.Cells.Max(c => c.X);
        int minY = reactorGrid.Cells.Min(c => c.Y);
        int maxY = reactorGrid.Cells.Max(c => c.Y);

        var gridDesign = new StringBuilder();

        for (int y = maxY; y >= minY; y--)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
                if (cell != null)
                {
                    gridDesign.Append(((int)cell.ColumnType).ToString());
                }
                else
                {
                    gridDesign.Append("-");
                }

                if (x < maxX)
                {
                    gridDesign.Append(" ");
                }
            }
            gridDesign.AppendLine();
        }

        return gridDesign.ToString();
    }

    public async Task<string> Get2DGridWithCoordinatesAsync(int reactorGridId, CancellationToken cancellationToken = default)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == reactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

        int minX = reactorGrid.Cells.Min(c => c.X);
        int maxX = reactorGrid.Cells.Max(c => c.X);
        int minY = reactorGrid.Cells.Min(c => c.Y);
        int maxY = reactorGrid.Cells.Max(c => c.Y);

        var gridDesign = new StringBuilder();

        for (int y = maxY; y >= minY; y--)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
                if (cell != null)
                {
                    gridDesign.Append($"({x},{y})");
                }
                else
                {
                    gridDesign.Append("-----");
                }

                if (x < maxX)
                {
                    gridDesign.Append(" ");
                }
            }
            gridDesign.AppendLine();
        }

        return gridDesign.ToString();
    }

    // Algo DFS :))
    public async Task<bool> IsReactorValidAsync(int reactorGridId, CancellationToken cancellationToken = default)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == reactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorGridId} not found.");

        if (reactorGrid.Cells.Count == 0)
        {
            return false;
        }

        int minX = reactorGrid.Cells.Min(c => c.X);
        int maxX = reactorGrid.Cells.Max(c => c.X);
        int minY = reactorGrid.Cells.Min(c => c.Y);
        int maxY = reactorGrid.Cells.Max(c => c.Y);

        var visited = new HashSet<(int, int)>();
        var stack = new Stack<(int, int)>();

        // Start DFS from the first cell
        var startCell = reactorGrid.Cells.First();
        stack.Push((startCell.X, startCell.Y));
        visited.Add((startCell.X, startCell.Y));

        while (stack.Count > 0)
        {
            var (currentX, currentY) = stack.Pop();

            // Explore all four possible directions: up, down, left, right
            foreach (var direction in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int newX = currentX + direction.Item1;
                int newY = currentY + direction.Item2;

                if (newX >= minX && newX <= maxX && newY >= minY && newY <= maxY)
                {
                    var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == newX && c.Y == newY);
                    if (cell != null && !visited.Contains((newX, newY)))
                    {
                        stack.Push((newX, newY));
                        visited.Add((newX, newY));
                    }
                }
            }
        }

        // If the number of visited cells equals the total number of cells, the reactor is valid
        return visited.Count == reactorGrid.Cells.Count;
    }
}