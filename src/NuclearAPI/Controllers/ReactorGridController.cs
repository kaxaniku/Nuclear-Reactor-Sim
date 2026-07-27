using Microsoft.AspNetCore.Mvc;
using Nuclear_Reactor_Sim.Models;
using NuclearApp.Interfaces.Services;
using NuclearDomain.DTOs;

namespace Nuclear_Reactor_Sim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReactorGridController : ControllerBase
{
    private readonly IReactorGridService _reactorGridService;

    public ReactorGridController(IReactorGridService reactorGridService)
    {
        _reactorGridService = reactorGridService ?? throw new ArgumentNullException(nameof(reactorGridService));
    }

    [HttpGet("cells")]
    public async Task<IActionResult> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default)
    {
        var cells = await _reactorGridService.GetAllCellsAsync(reactorGridId, cancellationToken);
        return Ok(cells);
    }

    [HttpGet("cellById")]
    public async Task<IActionResult> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default)
    {
        var cell = await _reactorGridService.GetCellByIdAsync(reactorGridId, cellId, cancellationToken);
        return Ok(cell);
    }

    [HttpGet("cellByCoordinates")]
    public async Task<IActionResult> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
    {
        var cell = await _reactorGridService.GetCellByCoordinatesAsync(reactorGridId, x, y, cancellationToken);
        return Ok(cell);
    }

    [HttpGet("getReactorIdByName")]
    public async Task<IActionResult> GetReactorIdByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var reactorId = await _reactorGridService.GetReactorGridIdByNameAsync(name, cancellationToken);
        return Ok(reactorId);
    }

    [HttpGet("getReactorById")]
    public async Task<IActionResult> GetReactorByIdAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var reactor = await _reactorGridService.GetReactorGridByIdAsync(reactorId, cancellationToken);
        return Ok(reactor);
    }

    [HttpGet("getAllReactors")]
    public async Task<IActionResult> GetAllReactorsAsync(CancellationToken cancellationToken = default)
    {
        var reactors = await _reactorGridService.GetAllReactorGridsAsync(cancellationToken);
        return Ok(reactors);
    }

    [HttpPost("insertCell")]
    public async Task<IActionResult> InsertCellAsync([FromBody] InsertCellRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConfigureCellCommandDto
        {
            Id = Guid.NewGuid(),
            X = request.X,
            Y = request.Y,
            NewColumnType = (ColumnType)request.NewColumnType
        };

        await _reactorGridService.InsertCellAsync(request.ReactorGridId, command, cancellationToken);
        return Ok();
    }

    [HttpPut("updateCell")]
    public async Task<IActionResult> UpdateCellAsync([FromBody] UpdateCellRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConfigureCellCommandDto
        {
            Id = Guid.NewGuid(),
            X = request.X,
            Y = request.Y,
            NewColumnType = (ColumnType)request.NewColumnType
        };
        await _reactorGridService.UpdateCellAsync(request.ReactorGridId, command, cancellationToken);
        return Ok();
    }

    [HttpDelete("deleteCell")]
    public async Task<IActionResult> DeleteCellAsync([FromBody] DeleteCellRequest request, CancellationToken cancellationToken = default)
    {
        await _reactorGridService.DeleteCellAsync(request.ReactorGridId, request.X, request.Y, cancellationToken);
        return Ok();
    }

    [HttpPost("createReactorGrid")]
    public async Task<IActionResult> CreateReactorGridAsync(string name, CancellationToken cancellationToken = default)
    {
        await _reactorGridService.CreateReactorAsync(name, cancellationToken);
        return Ok();
    }
}