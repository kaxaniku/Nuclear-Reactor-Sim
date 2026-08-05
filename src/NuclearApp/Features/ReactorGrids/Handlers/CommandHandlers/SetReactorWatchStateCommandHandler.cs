using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class SetReactorWatchStateCommandHandler : IRequestHandler<SetReactorWatchStateCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetReactorWatchStateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetReactorWatchStateCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken
        );

        var grid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        grid.IsMonitored = request.IsMonitored;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}