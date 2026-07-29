using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetCellByCoordinatesHandler : IRequestHandler<GetCellByCoordinatesQuery, Cell>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCellByCoordinatesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cell> Handle(GetCellByCoordinatesQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y) ?? throw new KeyNotFoundException($"Cell with coordinates {request.X}, {request.Y} not found.");
        return cell;
    }
}
