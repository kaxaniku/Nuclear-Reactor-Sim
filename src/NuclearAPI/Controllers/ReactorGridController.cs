using Microsoft.AspNetCore.Mvc;
using Nuclear_Reactor_Sim.Models;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Services;
using NuclearDomain.Entities;

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

    [HttpGet("reactorGrid/{reactorGridId:int}/cells/{cellId:int}")]
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

    [HttpGet("reactor/{reactorId:int}")]
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

        var cell = await _reactorGridService.InsertCellAsync(request.ReactorGridId, command, cancellationToken);
        var locationUri = $"/api/reactorGrid/{request.ReactorGridId}/cells/{cell.Id}";

        return Created(locationUri, cell);
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
        var updatedCell = await _reactorGridService.UpdateCellAsync(request.ReactorGridId, command, cancellationToken);
        return Ok(updatedCell);
    }

    [HttpDelete("deleteCell")]
    public async Task<IActionResult> DeleteCellAsync([FromBody] DeleteCellRequest request, CancellationToken cancellationToken = default)
    {
        await _reactorGridService.DeleteCellAsync(request.ReactorGridId, request.X, request.Y, cancellationToken);
        return NoContent();
    }

    [HttpDelete("deleteReactorById")]
    public async Task<IActionResult> DeleteReactorByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await _reactorGridService.DeleteReactorAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("createReactorGrid")]
    public async Task<IActionResult> CreateReactorGridAsync(string name, CancellationToken cancellationToken = default)
    {
        var reactor = await _reactorGridService.CreateReactorAsync(name, cancellationToken);
        var locationUri = $"/api/reactor/{reactor.Id}";

        return Created(locationUri, reactor);
    }

    [HttpGet("get2DGrid")]
    public async Task<IActionResult> Get2DGridAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var grid = await _reactorGridService.Get2DGridDesignAsync(reactorId, cancellationToken);
        return Ok(grid);
    }

    [HttpGet("get2DCoordinates")]
    public async Task<IActionResult> Get2DCoordinatesAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var coordinates = await _reactorGridService.Get2DGridWithCoordinatesAsync(reactorId, cancellationToken);
        return Ok(coordinates);
    }

    [HttpGet("validateReactor/{reactorId:int}")]
    public async Task<IActionResult> ValidateReactorAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var isValid = await _reactorGridService.IsReactorValidAsync(reactorId, cancellationToken);

        if (isValid)
            return Ok(new { IsValid = true });
        return BadRequest(new { IsValid = false });
    }
}