using System.ComponentModel.DataAnnotations;

namespace NuclearApp.DTOs;

public class ScramReactorCommandDto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!; // Added for consistency with other DTOs
    public string Reason { get; set; } = null!;
    public bool IsScrammed { get; set; } // Added to indicate the scram state
}
