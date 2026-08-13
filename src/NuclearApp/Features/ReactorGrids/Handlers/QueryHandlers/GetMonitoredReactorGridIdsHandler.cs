using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetMonitoredReactorGridIdsHandler : IRequestHandler<GetMonitoredReactorGridIdsQuery, List<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMonitoredReactorGridIdsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<int>> Handle(GetMonitoredReactorGridIdsQuery request, CancellationToken cancellationToken)
    {
        var monitoredGrids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.IsMonitored,
            cancellationToken
        );

        return monitoredGrids.Select(g => g.Id).ToList();
    }
}