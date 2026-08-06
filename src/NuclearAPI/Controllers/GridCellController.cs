using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuclear_Reactor_Sim.Models.Cells;
using NuclearApp.Features.GridCells;

namespace Nuclear_Reactor_Sim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GridCellController : ControllerBase
{
    private readonly IMediator _mediator;

    public GridCellController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("getCellTelemetry/{reactorGridId}/{x}/{y}")]
    public async Task<IActionResult> GetCellTelemetryAsync(int reactorGridId, int x, int y)
    {
        var query = new GetCellTelemetryQuery(reactorGridId, x, y);
        var cell = await _mediator.Send(query);
        return Ok(cell);
    }

    [HttpPost("moveControlRod")]
    public async Task<IActionResult> MoveControlRodAsync([FromBody] MoveControlRodRequest request, CancellationToken cancellationToken = default)
    {
        var command = new MoveControlRodCommand(request.ReactorGridId, request.X, request.Y, request.TargetInsertionPercentage);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("setGraphiteModeratorTemperature")]
    public async Task<IActionResult> SetGraphiteModeratorTemperatureAsync([FromBody] SetGraphiteModeratorTemperatureRequest request, CancellationToken cancellationToken = default)
    {
        var command = new SetGraphiteModeratorTemperatureCommand(request.ReactorGridId, request.X, request.Y, request.TemperatureCelsius);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("setAbsorberAbsorptionLevel")]
    public async Task<IActionResult> SetAbsorberAbsorptionLevelAsync([FromBody] SetAbsorberAbsorptionLevelRequest request, CancellationToken cancellationToken = default)
    {
        var command = new SetAbsorberAbsorptionLevelCommand(request.ReactorGridId, request.X, request.Y, request.AbsorptionLevelPercent);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("configureCooler")]
    public async Task<IActionResult> ConfigureCoolerAsync([FromBody] ConfigureCoolerRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConfigureCoolerCommand(request.ReactorGridId, request.X, request.Y, request.WaterFlowRate, request.CoolantLevelPercent);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("configureSteamChannel")]
    public async Task<IActionResult> ConfigureSteamChannelAsync([FromBody] ConfigureSteamChannelRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConfigureSteamChannelCommand(request.ReactorGridId, request.X, request.Y, request.SteamGenerationRateMW, request.PressureBar, request.Quality, request.Type);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("configureFuelChannel")]
    public async Task<IActionResult> ConfigureFuelChannelAsync([FromBody] ConfigureFuelChannelRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConfigureFuelChannelCommand(request.ReactorGridId, request.X, request.Y, request.NeutronFlux, request.LocalPowerOutputMW, request.Status);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("ToggleFuelRodActivation")]
    public async Task<IActionResult> ToggleFuelRodActivationAsync([FromBody] ToggleFuelRodActivationRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ToggleFuelRodActivationCommand(request.ReactorGridId, request.X, request.Y);
        var status = await _mediator.Send(command, cancellationToken);
        return Ok($"Fuel rod {(status ? "activated" : "deactivated")}.");
    }
}
