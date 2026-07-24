using System.ComponentModel.DataAnnotations;

namespace NuclearDomain.DTOs;

public class ConfigureCellCommandDto
{
    [Key]
    public Guid Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public ColumnType NewColumnType { get; set; }
}
