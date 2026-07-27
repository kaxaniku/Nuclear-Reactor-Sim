using NuclearDomain.DTOs;

namespace Nuclear_Reactor_Sim.Models;

public class UpdateCellRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int NewColumnType { get; set; }
}
