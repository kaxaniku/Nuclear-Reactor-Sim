using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells.Handlers.CommandHandlers;

public class MoveAllControlRodsCommandHandler : IRequestHandler<MoveAllControlRodsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MoveAllControlRodsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(MoveAllControlRodsCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        foreach (var cell in reactorGrid.Cells.Where(cell => cell.ColumnType == ColumnType.ControlRods))
        {
            var telemetry = cell.Telemetry as ControlRodsTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for control rods.");

            if (request.TargetInsertionPercentage < 0 || request.TargetInsertionPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(request.TargetInsertionPercentage), "Target insertion percentage must be between 0 and 100.");

            telemetry.TargetInsertionPercentage = request.TargetInsertionPercentage / 100.0;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ScramCommandHandler : IRequestHandler<ScramCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ScramCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ScramCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        foreach (var cell in reactorGrid.Cells.Where(cell => cell.ColumnType == ColumnType.ControlRods))
        {
            var telemetry = cell.Telemetry as ControlRodsTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for control rods.");

            telemetry.TargetInsertionPercentage = 1;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        foreach (var cell in reactorGrid.Cells.Where(c => c.ColumnType == ColumnType.SteamChannel))
        {
            var telemetry = cell.Telemetry as SteamChannelTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for steam channel.");

            telemetry.FlowRateThrottling = 1;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        foreach (var cell in reactorGrid.Cells.Where(c => c.ColumnType == ColumnType.FuelChannel))
        {
            var telemetry = cell.Telemetry as FuelChannelTelemetryDto
                ?? throw new InvalidOperationException("Invalid telemetry type for fuel rod.");
            telemetry.Status = FuelRodStatus.Scrammed;
            _unitOfWork.CellRepository.MarkModified(cell);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
