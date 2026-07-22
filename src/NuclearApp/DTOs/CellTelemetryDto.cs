using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NuclearApp.DTOs;

public enum SteamType
{
    Normal,         // 100C
    Dense,          // 300C
    SuperDense      // 450C
}


[JsonDerivedType(typeof(CellTelemetryDto), typeDiscriminator: "base")]
[JsonDerivedType(typeof(StructuralTelemetryDto), typeDiscriminator: "structural")]
[JsonDerivedType(typeof(GraphiteModeratorTelemetryDto), typeDiscriminator: "moderator")]
[JsonDerivedType(typeof(ControlRodsTelemetryDto), typeDiscriminator: "controlRods")]
[JsonDerivedType(typeof(CoolerTelemetryDto), typeDiscriminator: "cooler")]
[JsonDerivedType(typeof(SteamChannelTelemetryDto), typeDiscriminator: "steam")]
[JsonDerivedType(typeof(FuelChannelTelemetryDto), typeDiscriminator: "fuel")]
[JsonDerivedType(typeof(AbsorberTelemetryDto), typeDiscriminator: "absorber")]
public class CellTelemetryDto
{
    public double TemperatureCelsius { get; set; }   // Crucial for fuel and coolant channels
}

public class StructuralTelemetryDto : CellTelemetryDto
{
    // Add any specific structural telemetry fields here (if needed)
}

public class GraphiteModeratorTelemetryDto : CellTelemetryDto
{
    // No additional fields required beyond the base class
}

public class ControlRodsTelemetryDto : CellTelemetryDto
{
    public double InsertionLevel { get; set; }     // Percentage inserted 0.0 - 1.0
}

public class ReflectorTelemetryDto : CellTelemetryDto
{
    // No additional fields required beyond the base class
}

public class AbsorberTelemetryDto : CellTelemetryDto
{
    public double AbsorptionLevel { get; set; }   // Percentage absorbed 0.0 - 1.0
}

public class CoolerTelemetryDto : CellTelemetryDto
{
    public double WaterFlowRate { get; set; }       // Liters/sec through the individual channel
    public double CoolantLevelPercent { get; set; } // Percentage of coolant remaining (if needed)
}

public class SteamChannelTelemetryDto : CellTelemetryDto
{
    public double SteamGenerationRateMW { get; set; }
    public double PressureBar { get; set; }
    public double SteamQuality { get; set; }         // Percentage of water turned to steam (0.0 - 1.0)
    public SteamType SteamType { get; set; }
}

public class FuelChannelTelemetryDto : CellTelemetryDto
{
    public double NeutronFlux { get; set; }       // neutron flux rate
    public double LocalPowerOutputMW { get; set; }   // local thermal power generation
    public string Status { get; set; } = null!;      // Nominal, Warning, Scrammed, Meltdown
}