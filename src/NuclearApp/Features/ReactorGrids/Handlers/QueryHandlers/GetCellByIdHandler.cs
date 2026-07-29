using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetCellByIdHandler : IRequestHandler<GetCellByIdQuery, Cell>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCellByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cell> Handle(GetCellByIdQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");
        var cell = reactorGrid.Cells.FirstOrDefault(x => x.Id == request.CellId) ?? throw new KeyNotFoundException($"Cell with ID {request.CellId} not found.");
        return cell;
    }
}
