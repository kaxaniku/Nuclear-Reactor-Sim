using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ConfigureAllSteamChannelsCommandHandler : IRequestHandler<ConfigureAllSteamChannelsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureAllSteamChannelsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ConfigureAllSteamChannelsCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");


        if (reactorGrid.Cells.All(c => c.ColumnType != ColumnType.SteamChannel))
            throw new KeyNotFoundException($"No steam channels found on specified reactor {request.ReactorGridId}.");

        foreach (var cell in reactorGrid.Cells.Where(c => c.ColumnType == ColumnType.SteamChannel))
        {
            var telemetry = cell.Telemetry as SteamChannelTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for steam channel.");
            switch (request.Type)
            {
                case SteamType.Normal:
                    telemetry.TargetPressureBar = 1.0;
                    break;

                case SteamType.Dense:
                    telemetry.TargetPressureBar = 70.0;
                    break;

                case SteamType.Superheated:
                    telemetry.TargetPressureBar = 70.0;
                    break;

                case SteamType.Supercritical:
                    telemetry.TargetPressureBar = 225.0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Type), $"Unsupported steam type: {request.Type}");
            }
            telemetry.FlowRateThrottling = request.FlowRateThrottling;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}