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
        const double tickIntervalSeconds = 2.0;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(tickIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var watchedGridIds = await mediator.Send(new GetMonitoredReactorGridIdsQuery(), stoppingToken);

                foreach (var gridId in watchedGridIds)
                {
                    var overview = await mediator.Send(new GetReactorOverviewQuery(gridId), stoppingToken);

                    await mediator.Send(new ProcessReactorTickCommand(gridId, tickIntervalSeconds), stoppingToken);

                    Console.WriteLine($"\n========== REACTOR OVERVIEW [ID: {overview.ReactorId}] ==========");
                    Console.WriteLine($"Power: {overview.TotalPowerOutputMW:F2} MW | Avg Flux: {overview.AverageNeutronFlux:F2} | Fuel Channels: {overview.ActiveFuelChannels}");
                    Console.WriteLine($"Activation: {(overview.IsRunning ? "ONLINE" : "OFFLINE")}");
                    Console.WriteLine("----------------------------------------------------------");

                    string gridAscii = await mediator.Send(new Get2DGridDesignQuery(gridId), stoppingToken);
                    Console.WriteLine(gridAscii);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing monitored reactors loop.");
            }
        }
    }
}