using MediatR;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;

public class GetReactorOverviewHandler : IRequestHandler<GetReactorOverviewQuery, ReactorOverviewDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReactorOverviewHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ReactorOverviewDto> Handle(GetReactorOverviewQuery request, CancellationToken cancellationToken)
    {
        var grid = (await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        )).FirstOrDefault() ?? throw new KeyNotFoundException($"Grid {request.ReactorGridId} not found.");

        double totalPower = 0;
        double totalFlux = 0;
        int fuelCount = 0;

        foreach (var cell in grid.Cells)
        {
            if (cell.ColumnType == ColumnType.FuelChannel && cell.Telemetry is FuelChannelTelemetryDto fuel)
            {
                totalPower += fuel.LocalPowerOutputMW;
                totalFlux += fuel.NeutronFlux;
                fuelCount++;
            }
        }

        if (grid.Cells.Any(c => c.ColumnType == ColumnType.FuelChannel 
            && (c.Telemetry as FuelChannelTelemetryDto)!.IsOnline))
            grid.IsRunning = true;
        else 
            grid.IsRunning = false;

        return new ReactorOverviewDto(
            ReactorId: grid.Id,
            Name: grid.Name ?? "RBMK Reactor",
            AverageTemperature: 280.0,
            TotalPowerOutputMW: totalPower,
            AverageNeutronFlux: fuelCount > 0 ? totalFlux / fuelCount : 0,
            ActiveFuelChannels: fuelCount,
            IsRunning: grid.IsRunning
        );
    }
}
