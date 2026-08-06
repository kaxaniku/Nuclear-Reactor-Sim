using MediatR;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class MoveControlRodCommandHandler : IRequestHandler<MoveControlRodCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MoveControlRodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(MoveControlRodCommand request, CancellationToken cancellationToken)
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

        if (cell.ColumnType != ColumnType.ControlRods)
            throw new InvalidOperationException("The specified cell does not contain control rods.");

        var telemetry = cell.Telemetry as ControlRodsTelemetryDto
            ?? throw new InvalidOperationException("Invalid telemetry type for control rods.");

        if (request.TargetInsertionPercentage < 0 || request.TargetInsertionPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(request.TargetInsertionPercentage), "Target insertion percentage must be between 0 and 100.");

        cell.Telemetry = new ControlRodsTelemetryDto
        {
            TargetInsertionPercentage = request.TargetInsertionPercentage / 100.0
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
