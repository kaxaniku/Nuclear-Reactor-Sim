using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class IsReactorValidHandler : IRequestHandler<IsReactorValidQuery, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public IsReactorValidHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(IsReactorValidQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

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

        var startCell = reactorGrid.Cells.First();
        stack.Push((startCell.X, startCell.Y));
        visited.Add((startCell.X, startCell.Y));

        while (stack.Count > 0)
        {
            var (currentX, currentY) = stack.Pop();

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

        if (visited.Count == reactorGrid.Cells.Count)
        {
            reactorGrid.Validate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }
}
