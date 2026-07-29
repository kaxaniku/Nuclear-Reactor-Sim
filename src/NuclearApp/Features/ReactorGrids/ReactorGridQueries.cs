using MediatR;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids;

public record GetAllCellsQuery(int ReactorGridId) : IRequest<List<Cell>>;

public record GetCellByIdQuery(int ReactorGridId, int CellId) : IRequest<Cell>;

public record GetCellByCoordinatesQuery(int ReactorGridId, int X, int Y) : IRequest<Cell>;

public record GetReactorGridIdByNameQuery(string Name) : IRequest<int>;

public record GetAllReactorGridsQuery() : IRequest<IEnumerable<ReactorGrid>>;

public record GetReactorGridByIdQuery(int Id) : IRequest<ReactorGrid>;

public record Get2DGridDesignQuery(int ReactorGridId) : IRequest<string>;

public record Get2DGridWithCoordinatesQuery(int ReactorGridId) : IRequest<string>;

public record IsReactorValidQuery(int ReactorGridId) : IRequest<bool>;