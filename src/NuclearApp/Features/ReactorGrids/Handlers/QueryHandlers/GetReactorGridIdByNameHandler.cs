using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetReactorGridIdByNameHandler : IRequestHandler<GetReactorGridIdByNameQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReactorGridIdByNameHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(GetReactorGridIdByNameQuery request, CancellationToken cancellationToken)
    {
        var reactorGrid = (await _unitOfWork.ReactorGridRepository.QueryAsync(r => r.Name == request.Name && r.ActivityInfo.IsActive, cancellationToken))
            .FirstOrDefault() ?? throw new KeyNotFoundException($"Reactor grid with name {request.Name} not found.");
        return reactorGrid.Id;
    }
}
