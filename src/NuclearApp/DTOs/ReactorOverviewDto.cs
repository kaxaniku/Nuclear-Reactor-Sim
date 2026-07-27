using System.ComponentModel.DataAnnotations;

namespace NuclearApp.DTOs;

public class ReactorOverviewDto
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double TotalThermalPowerMW { get; set; }
    public double AverageCoolantTemp { get; set; }
    public double ControlRodAverageInsertion { get; set; }
    public double OperatingMargin { get; set; } // How close to unstable limits it is
    public bool IsScrammed { get; set; }
}
