using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ToggleFuelRodActivationHandler : IRequestHandler<ToggleFuelRodActivationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFuelRodActivationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ToggleFuelRodActivationCommand request, CancellationToken cancellationToken)
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
            throw new InvalidOperationException("The specified cell does not contain a fuel rod.");

        var telemetry = cell.Telemetry as FuelChannelTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for fuel rod.");

        bool activate = false;
        if (telemetry.IsOnline)
            activate = false;
        else
            activate = true;

        cell.Telemetry = new FuelChannelTelemetryDto
        {
            NeutronFlux = telemetry.NeutronFlux,
            LocalPowerOutputMW = telemetry.LocalPowerOutputMW,
            Status = telemetry.Status,
            IsOnline = activate,
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return activate;
    }
}