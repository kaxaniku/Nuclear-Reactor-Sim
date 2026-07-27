namespace Nuclear_Reactor_Sim.Models;

public class DeleteCellRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}