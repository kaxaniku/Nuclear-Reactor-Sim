using System.Text;
using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class Get2DGridDesignHandler : IRequestHandler<Get2DGridDesignQuery, string>
{
    private readonly IUnitOfWork _unitOfWork;

    public Get2DGridDesignHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(Get2DGridDesignQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

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
}
