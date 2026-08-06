using System.ComponentModel.DataAnnotations;

namespace NuclearDomain.Entities;

public class ReactorGrid
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public bool IsMonitored { get; set; }
    public bool IsRunning { get; set; } = false;
    public bool IsValid { get; set; } = false;
    public List<Cell> Cells { get; set; } = [];
    public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();

    public void Validate()
    {
        IsValid = true;
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}
