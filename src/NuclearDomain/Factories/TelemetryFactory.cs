namespace NuclearDomain.Factories;

using NuclearDomain.DTOs;

public static class TelemetryFactory
{
    public static CellTelemetryDto CreateDefault(ColumnType columnType) => columnType switch
    {
        ColumnType.FuelChannel => new FuelChannelTelemetryDto
        {
            TemperatureCelsius = 20.0, // Ambient starting temp
            NeutronFlux = 0.0,
            LocalPowerOutputMW = 0.0,
            Status = "Nominal"
        },
        ColumnType.ControlRods => new ControlRodsTelemetryDto
        {
            TemperatureCelsius = 20.0,
            InsertionLevel = 1.0 // Mandatory RBMK safety default: Fully inserted
        },
        ColumnType.Cooler => new CoolerTelemetryDto
        {
            TemperatureCelsius = 20.0,
            WaterFlowRate = 100.0,
            CoolantLevelPercent = 100.0
        },
        ColumnType.SteamChannel => new SteamChannelTelemetryDto
        {
            TemperatureCelsius = 20.0,
            SteamGenerationRateMW = 0.0,
            PressureBar = 1.0,
            SteamQuality = 0.0,
            SteamType = SteamType.Normal
        },
        ColumnType.Absorber => new AbsorberTelemetryDto
        {
            TemperatureCelsius = 20.0,
            AbsorptionLevel = 1.0
        },
        ColumnType.GraphiteModerator => new GraphiteModeratorTelemetryDto { TemperatureCelsius = 20.0 },
        ColumnType.Reflector => new ReflectorTelemetryDto { TemperatureCelsius = 20.0 },
        ColumnType.Structural => new StructuralTelemetryDto { TemperatureCelsius = 20.0 },
        _ => new CellTelemetryDto { TemperatureCelsius = 20.0 }
    };
}