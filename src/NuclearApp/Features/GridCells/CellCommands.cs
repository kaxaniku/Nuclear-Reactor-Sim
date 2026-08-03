using MediatR;
using NuclearDomain.Entities;

namespace NuclearApp.Features.GridCells;

public record MoveControlRodCommand(int ReactorGridId, int X, int Y, double TargetInsertionPercentage) : IRequest;
public record SetGraphiteModeratorTemperatureCommand(int ReactorGridId, int X, int Y, double TemperatureCelsius) : IRequest;
public record SetAbsorberAbsorptionLevelCommand(int ReactorGridId, int X, int Y, double AbsorptionLevelPercent) : IRequest;
public record ConfigureCoolerCommand(int ReactorGridId, int X, int Y, double WaterFlowRate, double CoolantLevelPercent) : IRequest;
public record ConfigureSteamChannelCommand(int ReactorGridId, int X, int Y, double SteamGenerationRateMW, double PressureBar, double Quality, SteamType Type) : IRequest;
public record ConfigureFuelChannelCommand(int ReactorGridId, int X, int Y, double NeutronFlux, double LocalPowerOutputMW, string Status) : IRequest;