using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class DeleteCellCommandHandler : IRequestHandler<DeleteCellCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCellCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCellCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y)
            ?? throw new KeyNotFoundException($"Cell at position ({request.X}, {request.Y}) not found in reactor grid.");

        reactorGrid.Cells.Remove(cell);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
