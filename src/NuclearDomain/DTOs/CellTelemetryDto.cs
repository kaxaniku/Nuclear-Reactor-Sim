using System.ComponentModel.DataAnnotations.Schema;

namespace NuclearDomain.DTOs;

[ComplexType]
public class CellTelemetryDto
{
    public double TemperatureCelsius { get; set; }   // Crucial for fuel and coolant channels
    public double WaterFlowRate { get; set; }        // Liters/sec through the individual channel
    public double SteamQuality { get; set; }         // Percentage of water turned to steam (0.0 - 1.0)
    public double? RodInsertionDepth { get; set; }   // Null if it's not a control rod channel (0.0 to 100.0%)
    public double LocalPowerOutputMW { get; set; }   // Local thermal power generation
    public string Status { get; set; } = null!;      // "Nominal", "Warning", "Scrammed", "Meltdown"
}
