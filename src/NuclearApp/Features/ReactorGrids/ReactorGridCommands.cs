using MediatR;
using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Features.ReactorGrids;

public record InsertCellCommand(int ReactorGridId, ConfigureCellCommandDto Command) : IRequest<Cell>;

public record UpdateCellCommand(int ReactorGridId, ConfigureCellCommandDto Command) : IRequest<Cell>;

public record DeleteCellCommand(int ReactorGridId, int X, int Y) : IRequest;

public record CreateReactorCommand(string Name) : IRequest<ReactorGrid>;

public record DeleteReactorCommand(int Id) : IRequest;

public record SetReactorWatchStateCommand(int ReactorGridId, bool IsMonitored) : IRequest, IRequiresValidReactor;

public record ResetReactorCommand(int ReactorGridId) : IRequest;

public record ProcessReactorTickCommand(int ReactorGridId, double DeltaTimeSeconds) : IRequest;