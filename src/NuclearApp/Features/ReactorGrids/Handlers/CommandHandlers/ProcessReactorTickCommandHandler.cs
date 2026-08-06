using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearApp.Interfaces.Services;
using NuclearApp.Services;

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
        )).FirstOrDefault();

        if (grid == null || !grid.IsRunning || !grid.IsValid)
            return;

        _physicsEngine.ProcessPhysicsTick(grid, request.DeltaTimeSeconds);
        _unitOfWork.CellRepository.MarkRangeModified(grid.Cells);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}