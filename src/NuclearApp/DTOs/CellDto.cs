using System.ComponentModel.DataAnnotations;

namespace NuclearApp.DTOs;

public class CellDto
{
    [Key]
    public int Id { get; set; }
    public int X { get; set; } // Adjusted from RowIndex
    public int Y { get; set; } // Adjusted from ColumnIndex
    public string ColumnType { get; set; } = null!; // "Fuel", "ControlRod", "Cooler", "Reflector"
    public CellTelemetryDto Telemetry { get; set; } = new CellTelemetryDto();
    public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();
}