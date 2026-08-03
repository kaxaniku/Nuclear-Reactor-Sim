using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuclear_Reactor_Sim.Models.NuclearGrid;
using NuclearApp.DTOs;
using NuclearApp.Features.ReactorGrids;
using NuclearDomain.Entities;

namespace Nuclear_Reactor_Sim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReactorGridController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReactorGridController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("cells")]
    public async Task<IActionResult> GetAllCellsAsync(int reactorGridId, CancellationToken cancellationToken = default)
    {
        var query = new GetAllCellsQuery(reactorGridId);
        var cells = await _mediator.Send(query, cancellationToken);
        return Ok(cells);
    }

    [HttpGet("reactorGrid/{reactorGridId:int}/cells/{cellId:int}")]
    public async Task<IActionResult> GetCellByIdAsync(int reactorGridId, int cellId, CancellationToken cancellationToken = default)
    {
        var query = new GetCellByIdQuery(reactorGridId, cellId);
        var cell = await _mediator.Send(query, cancellationToken);
        return Ok(cell);
    }

    [HttpGet("cellByCoordinates")]
    public async Task<IActionResult> GetCellByCoordinatesAsync(int reactorGridId, int x, int y, CancellationToken cancellationToken = default)
    {
        var query = new GetCellByCoordinatesQuery(reactorGridId, x, y);
        var cell = await _mediator.Send(query, cancellationToken);
        return Ok(cell);
    }

    [HttpGet("getReactorIdByName")]
    public async Task<IActionResult> GetReactorIdByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var query = new GetReactorGridIdByNameQuery(name);
        var reactorId = await _mediator.Send(query, cancellationToken);
        return Ok(reactorId);
    }

    [HttpGet("reactor/{reactorId:int}")]
    public async Task<IActionResult> GetReactorByIdAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var query = new GetReactorGridByIdQuery(reactorId);
        var reactor = await _mediator.Send(query, cancellationToken);
        return Ok(reactor);
    }

    [HttpGet("getAllReactors")]
    public async Task<IActionResult> GetAllReactorsAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetAllReactorGridsQuery();
        var reactors = await _mediator.Send(query, cancellationToken);
        return Ok(reactors);
    }

    [HttpPost("insertCell")]
    public async Task<IActionResult> InsertCellAsync([FromBody] InsertCellRequest request, CancellationToken cancellationToken = default)
    {
        var commandDto = new ConfigureCellCommandDto
        {
            Id = Guid.NewGuid(),
            X = request.X,
            Y = request.Y,
            NewColumnType = (ColumnType)request.NewColumnType
        };
        
        var command = new InsertCellCommand(request.ReactorGridId ,commandDto);

        var cell = await _mediator.Send(command, cancellationToken);

        var locationUri = $"/api/reactorGrid/{request.ReactorGridId}/cells/{cell.Id}";

        return Created(locationUri, cell);
    }

    [HttpPut("updateCell")]
    public async Task<IActionResult> UpdateCellAsync([FromBody] UpdateCellRequest request, CancellationToken cancellationToken = default)
    {
        var commandDto = new ConfigureCellCommandDto
        {
            Id = Guid.NewGuid(),
            X = request.X,
            Y = request.Y,
            NewColumnType = (ColumnType)request.NewColumnType
        };
        
        var command = new UpdateCellCommand(request.ReactorGridId ,commandDto);

        var cell = await _mediator.Send(command, cancellationToken);

        return Ok(cell);
    }

    [HttpDelete("deleteCell")]
    public async Task<IActionResult> DeleteCellAsync([FromBody] DeleteCellRequest request, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteCellCommand(request.ReactorGridId ,request.X, request.Y), cancellationToken);
        return NoContent();
    }

    [HttpDelete("deleteReactorById")]
    public async Task<IActionResult> DeleteReactorByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteReactorCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("createReactorGrid")]
    public async Task<IActionResult> CreateReactorGridAsync(string name, CancellationToken cancellationToken = default)
    {
        var command = new CreateReactorCommand(name);
        var reactor = await _mediator.Send(command, cancellationToken);
        var locationUri = $"/api/reactor/{reactor.Id}";

        return Created(locationUri, reactor);
    }

    [HttpGet("get2DGrid")]
    public async Task<IActionResult> Get2DGridAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var query = new Get2DGridDesignQuery(reactorId);
        var grid = await _mediator.Send(query, cancellationToken);
        
        return Ok(grid);
    }

    [HttpGet("get2DCoordinates")]
    public async Task<IActionResult> Get2DCoordinatesAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var query = new Get2DGridWithCoordinatesQuery(reactorId);
        var coordinates = await _mediator.Send(query, cancellationToken);
        
        return Ok(coordinates);
    }

    [HttpGet("validateReactor/{reactorId:int}")]
    public async Task<IActionResult> ValidateReactorAsync(int reactorId, CancellationToken cancellationToken = default)
    {
        var query = new IsReactorValidQuery(reactorId);
        var isValid = await _mediator.Send(query, cancellationToken);

        if (isValid)
            return Ok(new { IsValid = true });
        return BadRequest(new { IsValid = false });
    }
}