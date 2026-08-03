using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class SetAbsorberAbsorptionLevelCommandHandler : IRequestHandler<SetAbsorberAbsorptionLevelCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetAbsorberAbsorptionLevelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(SetAbsorberAbsorptionLevelCommand request, CancellationToken cancellationToken)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(request.ReactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({request.X}, {request.Y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.Absorber)
            throw new InvalidOperationException("The specified cell does not contain an absorber.");

        var telemetry = cell.Telemetry as AbsorberTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for absorber.");

        if (request.AbsorptionLevelPercent < 0 || request.AbsorptionLevelPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(request.AbsorptionLevelPercent), "Absorption level must be between 0 and 100.");

        telemetry.AbsorptionLevel = request.AbsorptionLevelPercent / 100.0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
