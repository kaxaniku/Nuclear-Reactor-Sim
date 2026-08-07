using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ConfigureFuelChannelCommandHandler : IRequestHandler<ConfigureFuelChannelCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureFuelChannelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ConfigureFuelChannelCommand request, CancellationToken cancellationToken)
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

        if (cell.ColumnType != ColumnType.FuelChannel)
            throw new InvalidOperationException("The specified cell does not contain a fuel channel.");

        var telemetry = cell.Telemetry as FuelChannelTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for fuel channel.");

        if (telemetry.IsOnline)
            throw new InvalidOperationException("Can not configure running fuel channel.");

        cell.Telemetry = new FuelChannelTelemetryDto
        {
            Status = request.Status
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}