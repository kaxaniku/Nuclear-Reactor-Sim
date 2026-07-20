using System.ComponentModel.DataAnnotations;

public class ConfigureCellCommandDto
{
    [Key]
    public Guid Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string NewColumnType { get; set; } = null!; // Changes an empty space into a "Fuel" or "ControlRod" channel
}
