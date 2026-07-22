using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NuclearApp.DTOs;

public enum ColumnType
{
    Structural,
    GraphiteModerator,
    ControlRods,
    Reflector,
    Absorber,
    Cooler,
    SteamChannel,
    FuelChannel
}

public class CellDto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int X { get; set; }

    [Required]
    public int Y { get; set; }

    [Required]
    public ColumnType ColumnType { get; set; }

    public CellTelemetryDto Telemetry { get; set; } = new CellTelemetryDto();
}