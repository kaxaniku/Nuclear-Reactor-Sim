using System.ComponentModel.DataAnnotations;

namespace NuclearDomain.DTOs;

public class MoveControlRodCommandDto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int X { get; set; }
    public int Y { get; set; }
    public double TargetInsertionPercentage { get; set; } // 0.0 (fully extracted) to 100.0 (fully inserted)
}
