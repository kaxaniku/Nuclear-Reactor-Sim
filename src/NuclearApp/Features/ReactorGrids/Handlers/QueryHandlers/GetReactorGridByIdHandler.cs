using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetReactorGridByIdHandler : IRequestHandler<GetReactorGridByIdQuery, ReactorGrid>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReactorGridByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReactorGrid> Handle(GetReactorGridByIdQuery request, CancellationToken cancellationToken)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Reactor grid with ID {request.Id} not found.");
        return reactorGrid;
    }
}