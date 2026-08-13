using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class ToggleAllFuelRodsHandler : IRequestHandler<ToggleAllFuelRodsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleAllFuelRodsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleAllFuelRodsCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        foreach (var cell in reactorGrid.Cells.Where(c => c.ColumnType == ColumnType.FuelChannel))
        {
            var telemetry = cell.Telemetry as FuelChannelTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for fuel rod.");
            bool activate = false;
            if (telemetry.IsOnline)
                activate = false;
            else
                activate = true;
            telemetry.IsOnline = activate;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}