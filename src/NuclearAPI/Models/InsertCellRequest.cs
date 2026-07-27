namespace Nuclear_Reactor_Sim.Models;

public class InsertCellRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int NewColumnType { get; set; }
}
