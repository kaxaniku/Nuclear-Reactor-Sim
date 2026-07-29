using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetAllCellsHandler : IRequestHandler<GetAllCellsQuery, List<Cell>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCellsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Cell>> Handle(GetAllCellsQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        return reactorGrid.Cells.ToList();
    }
}