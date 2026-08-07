using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Factories;

namespace NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;

public class InsertCellCommandHandler : IRequestHandler<InsertCellCommand, Cell>
{
    private readonly IUnitOfWork _unitOfWork;

    public InsertCellCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cell> Handle(InsertCellCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        if (reactorGrid.Cells.Any(c => c.X == request.Command.X && c.Y == request.Command.Y))
        {
            throw new ArgumentException("Cell already exists at the specified coordinates.");
        }

        var cell = new Cell
        {
            X = request.Command.X,
            Y = request.Command.Y,
            ColumnType = request.Command.NewColumnType,
            Telemetry = TelemetryFactory.CreateDefault(request.Command.NewColumnType),
            ReactorGridId = reactorGrid.Id
        };

        reactorGrid.Cells.Add(cell);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cell;
    }
}

public class ResetReactorCommandHandler : IRequestHandler<ResetReactorCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ResetReactorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetReactorCommand request, CancellationToken cancellationToken)
    {
        var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
            g => g.Id == request.ReactorGridId,
            cancellationToken,
            g => g.Cells
        );

        var reactorGrid = grids.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Reactor grid with ID {request.ReactorGridId} not found.");

        reactorGrid.Cells.ForEach(cell => cell.Telemetry = TelemetryFactory.CreateDefault(cell.ColumnType));

        _unitOfWork.CellRepository.MarkRangeModified(reactorGrid.Cells);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
