using NuclearDomain.Entities;

namespace NuclearApp.DTOs;

public class ConfigureCellCommandDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int X { get; set; }
    public int Y { get; set; }
    public ColumnType NewColumnType { get; set; }
}
