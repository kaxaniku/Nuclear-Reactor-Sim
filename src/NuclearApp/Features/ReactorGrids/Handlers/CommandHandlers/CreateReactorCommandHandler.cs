using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class CreateReactorCommandHandler : IRequestHandler<CreateReactorCommand, ReactorGrid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReactorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReactorGrid> Handle(CreateReactorCommand request, CancellationToken cancellationToken)
    {
        var reactorGrid = new ReactorGrid
        {
            Name = request.Name,
            Cells = new List<Cell>(),
            TotalRows = 0,
            TotalColumns = 0
        };

        await _unitOfWork.ReactorGridRepository.InsertAsync(reactorGrid, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reactorGrid;
    }
}
