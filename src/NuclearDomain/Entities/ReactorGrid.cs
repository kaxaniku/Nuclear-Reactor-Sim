using System.ComponentModel.DataAnnotations;

namespace NuclearDomain.Entities;

public class ReactorGrid
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public List<Cell> Cells { get; set; } = [];
    public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();
}
