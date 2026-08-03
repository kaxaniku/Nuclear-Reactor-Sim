namespace Nuclear_Reactor_Sim.Models.Cells;

public class MoveControlRodRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double TargetInsertionPercentage { get; set; }
}
