using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class DeleteReactorCommandHandler : IRequestHandler<DeleteReactorCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReactorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteReactorCommand request, CancellationToken cancellationToken)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.Id} not found.");

        _unitOfWork.ReactorGridRepository.Delete(reactorGrid);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
