using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ConfigureSteamChannelCommandHandler : IRequestHandler<ConfigureSteamChannelCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureSteamChannelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ConfigureSteamChannelCommand request, CancellationToken cancellationToken)
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

        if (cell.ColumnType != ColumnType.SteamChannel)
            throw new InvalidOperationException("The specified cell does not contain a steam channel.");

        var telemetry = cell.Telemetry as SteamChannelTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for steam channel.");

        switch (request.Type)
        {
            case SteamType.Normal:
                telemetry.TargetPressureBar = 1.0;
                telemetry.FlowRateThrottling = 1.0;
                break;

            case SteamType.Dense:
                telemetry.TargetPressureBar = 70.0;
                telemetry.FlowRateThrottling = 1.0;
                break;

            case SteamType.Superheated:
                telemetry.TargetPressureBar = 70.0;
                // Throttling coolant flow to 25% forces water to dwell long enough to reach 100% steam quality and superheat
                telemetry.FlowRateThrottling = 0.25;
                break;

            case SteamType.Supercritical:
                telemetry.TargetPressureBar = 225.0; // Above critical point threshold (221.2 Bar)
                telemetry.FlowRateThrottling = 0.8;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(request.Type), $"Unsupported steam type: {request.Type}");
        }

        _unitOfWork.CellRepository.MarkRangeModified(new[] { cell });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}