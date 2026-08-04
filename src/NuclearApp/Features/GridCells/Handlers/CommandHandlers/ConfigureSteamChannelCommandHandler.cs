using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

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

        telemetry.SteamGenerationRateMW = request.SteamGenerationRateMW;
        telemetry.PressureBar = request.PressureBar;
        telemetry.SteamQuality = request.Quality;
        telemetry.SteamType = request.Type;

        cell.Telemetry = new SteamChannelTelemetryDto
        {
            SteamGenerationRateMW = request.SteamGenerationRateMW,
            PressureBar = request.PressureBar,
            SteamQuality = request.Quality,
            SteamType = request.Type
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
