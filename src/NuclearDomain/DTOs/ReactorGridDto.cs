using System.ComponentModel.DataAnnotations;

namespace NuclearDomain.DTOs;

public class ReactorGridDto
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public List<CellDto> Cells { get; set; } = [];
    public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();
}
