using MediatR;
using NuclearApp.Features.ReactorGrids;
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
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y)
            ?? throw new InvalidOperationException($"Cell at position ({request.X}, {request.Y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.GraphiteModerator)
            throw new InvalidOperationException("The specified cell does not contain a graphite moderator.");

        var telemetry = cell.Telemetry as GraphiteModeratorTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for graphite moderator.");

        cell.Telemetry = new GraphiteModeratorTelemetryDto
        {
            TemperatureCelsius = telemetry.TemperatureCelsius
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
