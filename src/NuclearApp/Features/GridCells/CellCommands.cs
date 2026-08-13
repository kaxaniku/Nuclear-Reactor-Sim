using MediatR;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Features.GridCells;

public record MoveControlRodCommand(int ReactorGridId, int X, int Y, double TargetInsertionPercentage) : IRequest, IRequiresValidReactor;
public record MoveAllControlRodsCommand(int ReactorGridId, double TargetInsertionPercentage) : IRequest, IRequiresValidReactor;
public record SetGraphiteModeratorTemperatureCommand(int ReactorGridId, int X, int Y, double TemperatureCelsius) : IRequest, IRequiresValidReactor;
public record ConfigureCoolerCommand(int ReactorGridId, int X, int Y, double WaterFlowRate, double CoolantLevelPercent) : IRequest, IRequiresValidReactor;
public record ConfigureSteamChannelCommand(int ReactorGridId, int X, int Y, SteamType Type, double FlowRateThrottling) : IRequest, IRequiresValidReactor;
public record ConfigureAllSteamChannelsCommand(int ReactorGridId, SteamType Type, double FlowRateThrottling) : IRequest, IRequiresValidReactor;
public record ConfigureFuelChannelCommand(int ReactorGridId, int X, int Y, FuelRodStatus Status) : IRequest, IRequiresValidReactor;
public record ToggleFuelRodActivationCommand(int ReactorGridId, int X, int Y) : IRequest<bool>, IRequiresValidReactor;
public record ToggleAllFuelRodsCommand(int ReactorGridId) : IRequest, IRequiresValidReactor;
public record ScramCommand(int ReactorGridId) : IRequest, IRequiresValidReactor;