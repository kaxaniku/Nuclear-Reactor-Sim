using MediatR;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells;

public record GetCellTelemetryQuery(int ReactorGridId, int X, int Y) : IRequest<CellTelemetry>;
public record GetReactorGridCellsQuery(int ReactorGridId) : IRequest<List<Cell>>;
