using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NuclearDomain.Entities.Telemetries;

namespace NuclearDomain.Entities;

public enum ColumnType
{
    Structural = 0,
    GraphiteModerator = 1,
    ControlRods = 2,
    Reflector = 3,
    Absorber = 4,
    Cooler = 5,
    SteamChannel = 6,
    FuelChannel = 7
}

public class Cell
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int X { get; set; }

    [Required]
    public int Y { get; set; }

    [Required]
    public ColumnType ColumnType { get; set; }

    public CellTelemetry Telemetry { get; set; } = new CellTelemetry();

    public int ReactorGridId { get; set; }

    [ForeignKey(nameof(ReactorGridId))]
    [JsonIgnore]
    public ReactorGrid ReactorGrid { get; set; } = null!;
}