using MediatR;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

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

        double totalPower = 0.0;
        double totalSteamGenerationMW = 0.0;
        double totalFlux = 0.0;
        double totalTemperature = 0.0;

        int fuelCount = 0;
        int activeFuelCount = 0;
        int totalCells = grid.Cells.Count;

        foreach (var cell in grid.Cells)
        {
            if (cell.Telemetry is CellTelemetry baseTelemetry)
            {
                totalTemperature += baseTelemetry.TemperatureCelsius;
            }

            if (cell.ColumnType == ColumnType.FuelChannel && cell.Telemetry is FuelChannelTelemetryDto fuel)
            {
                fuelCount++;

                if (fuel.IsOnline && fuel.Status != FuelRodStatus.Meltdown)
                {
                    activeFuelCount++;
                    totalPower += fuel.LocalPowerOutputMW;
                    totalFlux += (fuel.ThermalFlux + fuel.FastFlux);
                }
            }
            else if (cell.ColumnType == ColumnType.SteamChannel && cell.Telemetry is SteamChannelTelemetryDto steam)
            {
                totalSteamGenerationMW += steam.SteamGenerationRateMW;
            }
        }

        double averageTemperature = totalCells > 0 ? totalTemperature / totalCells : 20.0;
        double averageNeutronFlux = activeFuelCount > 0 ? totalFlux / activeFuelCount : 0.0;

        return new ReactorOverviewDto(
            ReactorId: grid.Id,
            Name: grid.Name ?? "RBMK Reactor",
            AverageTemperature: averageTemperature,
            TotalPowerOutputMW: totalPower,
            TotalSteamGenerationMW: totalSteamGenerationMW,
            AverageNeutronFlux: averageNeutronFlux,
            ActiveFuelChannels: activeFuelCount,
            TotalFuelChannels: fuelCount,
            IsRunning: grid.IsRunning
        );
    }
}