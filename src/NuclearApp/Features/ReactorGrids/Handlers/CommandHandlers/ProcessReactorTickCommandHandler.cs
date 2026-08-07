using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearApp.Interfaces.Services;
using NuclearApp.Services;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class ProcessReactorTickCommandHandler : IRequestHandler<ProcessReactorTickCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReactorPhysicsEngine _physicsEngine;

    public ProcessReactorTickCommandHandler(IUnitOfWork unitOfWork, IReactorPhysicsEngine physicsEngine)
    {
        _unitOfWork = unitOfWork;
        _physicsEngine = physicsEngine;
    }

    public async Task Handle(ProcessReactorTickCommand request, CancellationToken cancellationToken)
    {
        var grid = (await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        )).FirstOrDefault() ?? throw new Exception("Reactor grid not found");

        int activeFuelCount = grid.Cells.Count(c =>
            c.ColumnType == ColumnType.FuelChannel &&
            c.Telemetry is FuelChannelTelemetryDto fuel &&
            fuel.IsOnline &&
            fuel.Status != FuelRodStatus.Meltdown);

        // Update grid state deterministically during the physics tick
        grid.IsRunning = activeFuelCount > 0;

        if (!grid.IsRunning || !grid.IsValid)
        {
            _physicsEngine.ProcessPhysicsTick(grid, request.DeltaTimeSeconds);
            _unitOfWork.CellRepository.MarkRangeModified(grid.Cells);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}