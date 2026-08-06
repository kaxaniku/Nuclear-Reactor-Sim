using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NuclearDomain.Entities.Telemetries;

public enum SteamType
{
    Normal,         // 100C
    Dense,          // 300C
    SuperDense      // 450C
}

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
    public double AbsorptionLevel { get; set; }   // Percentage absorbed 0.0 - 1.0
}

public class CoolerTelemetryDto : CellTelemetry
{
    public double WaterFlowRate { get; set; }       // Liters/sec through the individual channel
    public double CoolantLevelPercent { get; set; } // Percentage of coolant remaining (if needed)
}

public class SteamChannelTelemetryDto : CellTelemetry
{
    public double SteamGenerationRateMW { get; set; }
    public double PressureBar { get; set; }
    public double SteamQuality { get; set; }         // Percentage of water turned to steam (0.0 - 1.0)
    public SteamType SteamType { get; set; }
}
