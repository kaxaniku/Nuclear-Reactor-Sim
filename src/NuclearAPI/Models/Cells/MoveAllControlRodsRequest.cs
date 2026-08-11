namespace Nuclear_Reactor_Sim.Models.Cells;

public class MoveAllControlRodsRequest
{
    public int ReactorGridId { get; set; }
    public double TargetInsertionPercentage { get; set; }
}
