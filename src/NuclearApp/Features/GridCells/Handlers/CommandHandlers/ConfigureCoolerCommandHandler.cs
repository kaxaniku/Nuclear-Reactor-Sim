using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ConfigureCoolerCommandHandler : IRequestHandler<ConfigureCoolerCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureCoolerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ConfigureCoolerCommand request, CancellationToken cancellationToken)
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

        if (cell.ColumnType != ColumnType.Cooler)
            throw new InvalidOperationException("The specified cell does not contain a cooler.");

        var telemetry = cell.Telemetry as CoolerTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for cooler.");

        if (request.CoolantLevelPercent < 0 || request.CoolantLevelPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(request.CoolantLevelPercent), "Coolant level percentage must be between 0 and 100.");

        cell.Telemetry = new CoolerTelemetryDto
        {
            WaterFlowRate = request.WaterFlowRate,
            CoolantLevelPercent = request.CoolantLevelPercent / 100.0
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
