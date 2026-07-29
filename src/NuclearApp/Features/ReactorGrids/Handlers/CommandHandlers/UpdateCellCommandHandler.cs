using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Factories;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class UpdateCellCommandHandler : IRequestHandler<UpdateCellCommand, Cell>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCellCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cell> Handle(UpdateCellCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.Command.X && c.Y == request.Command.Y)
            ?? throw new KeyNotFoundException($"Cell at position ({request.Command.X}, {request.Command.Y}) not found in reactor grid.");

        if (cell.ColumnType != request.Command.NewColumnType)
        {
            cell.ColumnType = request.Command.NewColumnType;
            cell.Telemetry = TelemetryFactory.CreateDefault(request.Command.NewColumnType);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cell;
    }
}
