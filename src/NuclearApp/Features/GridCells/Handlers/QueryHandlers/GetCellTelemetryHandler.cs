using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.QueryHandlers;

public class GetCellTelemetryHandler : IRequestHandler<GetCellTelemetryQuery, CellTelemetry>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCellTelemetryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CellTelemetry> Handle(GetCellTelemetryQuery request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y)
            ?? throw new KeyNotFoundException($"Cell at coordinates ({request.X}, {request.Y}) not found in reactor grid with ID {request.ReactorGridId}.");

        return cell.Telemetry;
    }
}
