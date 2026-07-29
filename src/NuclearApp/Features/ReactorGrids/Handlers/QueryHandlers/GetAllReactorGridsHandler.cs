using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetAllReactorGridsHandler : IRequestHandler<GetAllReactorGridsQuery, IEnumerable<ReactorGrid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllReactorGridsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ReactorGrid>> Handle(GetAllReactorGridsQuery request, CancellationToken cancellationToken)
    {
        var reactorsGrids = await _unitOfWork.ReactorGridRepository.QueryAsync(r => r.ActivityInfo.IsActive, cancellationToken);
        return reactorsGrids;
    }
}
