using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NuclearDomain.Entities.Telemetries;

[JsonDerivedType(typeof(CellTelemetry), typeDiscriminator: "base")]
[JsonDerivedType(typeof(StructuralTelemetryDto), typeDiscriminator: "structural")]
[JsonDerivedType(typeof(GraphiteModeratorTelemetryDto), typeDiscriminator: "moderator")]
[JsonDerivedType(typeof(ControlRodsTelemetryDto), typeDiscriminator: "controlRods")]
[JsonDerivedType(typeof(ReflectorTelemetryDto), typeDiscriminator: "reflector")]
[JsonDerivedType(typeof(CoolerTelemetryDto), typeDiscriminator: "cooler")]
[JsonDerivedType(typeof(SteamChannelTelemetryDto), typeDiscriminator: "steam")]
[JsonDerivedType(typeof(FuelChannelTelemetryDto), typeDiscriminator: "fuel")]
[JsonDerivedType(typeof(AbsorberTelemetryDto), typeDiscriminator: "absorber")]
public class CellTelemetry
{
    public double TemperatureCelsius { get; set; }   // Crucial for fuel and coolant channels
    public double FastFlux { get; set; }
    public double ThermalFlux { get; set; }
}

public class StructuralTelemetryDto : CellTelemetry
{
    // Add any specific structural telemetry fields here (if needed)
}

public class GraphiteModeratorTelemetryDto : CellTelemetry
{
    // No additional fields required beyond the base class
}

public class ReflectorTelemetryDto : CellTelemetry
{
    // No additional fields required beyond the base class
}

public class AbsorberTelemetryDto : CellTelemetry
{
}

public class CoolerTelemetryDto : CellTelemetry
{
    public double WaterFlowRate { get; set; }       // Liters/sec through the individual channel
    public double CoolantLevelPercent { get; set; } // Percentage of coolant remaining (if needed)
}
