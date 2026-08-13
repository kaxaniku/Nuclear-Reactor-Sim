using MediatR;
using NuclearApp.Features.ReactorGrids;

namespace NuclearEngine.Workers;

public class ReactorSimulationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReactorSimulationWorker> _logger;

    public ReactorSimulationWorker(IServiceScopeFactory scopeFactory, ILogger<ReactorSimulationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const double renderIntervalSeconds = 2.0;
        const int subTicksPerInterval = 20; // Run physics 10 times per render cycle
        const double physicsSubTickSeconds = renderIntervalSeconds / subTicksPerInterval; // 0.1s physics step

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(renderIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<int> watchedGridIds;

                // Query monitored IDs
                using (var initScope = _scopeFactory.CreateScope())
                {
                    var mediator = initScope.ServiceProvider.GetRequiredService<IMediator>();
                    watchedGridIds = (await mediator.Send(new GetMonitoredReactorGridIdsQuery(), stoppingToken)).ToList();
                }

                foreach (var gridId in watchedGridIds)
                {
                    using (var commandScope = _scopeFactory.CreateScope())
                    {
                        var mediator = commandScope.ServiceProvider.GetRequiredService<IMediator>();

                        for (int i = 0; i < subTicksPerInterval; i++)
                        {
                            await mediator.Send(new ProcessReactorTickCommand(gridId, physicsSubTickSeconds), stoppingToken);
                        }
                    }

                    using (var queryScope = _scopeFactory.CreateScope())
                    {
                        var mediator = queryScope.ServiceProvider.GetRequiredService<IMediator>();
                        var overview = await mediator.Send(new GetReactorOverviewQuery(gridId), stoppingToken);
                        string gridAscii = await mediator.Send(new Get2DGridDesignQuery(gridId), stoppingToken);

                        Console.WriteLine($"\n========== REACTOR OVERVIEW [ID: {overview.ReactorId}] ==========");
                        Console.WriteLine($"Power: {overview.TotalPowerOutputMW:F2} MW | Steam: {overview.TotalSteamGenerationMW:F2} MW");
                        Console.WriteLine($"Avg Temp: {overview.AverageTemperature:F1} °C | Avg Flux: {overview.AverageNeutronFlux:F2}");
                        Console.WriteLine($"Active Fuel: {overview.ActiveFuelChannels}/{overview.TotalFuelChannels} | Status: {(overview.IsRunning ? "ONLINE" : "OFFLINE")}");
                        Console.WriteLine("----------------------------------------------------------");
                        Console.WriteLine(gridAscii);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing physics tick for monitored reactor grids.");
            }
        }
    }
}