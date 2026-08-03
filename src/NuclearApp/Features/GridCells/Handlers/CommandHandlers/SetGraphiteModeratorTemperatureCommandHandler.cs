using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class SetGraphiteModeratorTemperatureCommandHandler : IRequestHandler<SetGraphiteModeratorTemperatureCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetGraphiteModeratorTemperatureCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(SetGraphiteModeratorTemperatureCommand request, CancellationToken cancellationToken)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(request.ReactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({request.X}, {request.Y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.GraphiteModerator)
            throw new InvalidOperationException("The specified cell does not contain a graphite moderator.");

        var telemetry = cell.Telemetry as CellTelemetry;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for graphite moderator.");

        telemetry.TemperatureCelsius = request.TemperatureCelsius;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
