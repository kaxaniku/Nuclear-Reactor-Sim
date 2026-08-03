namespace Nuclear_Reactor_Sim.Models.Cells;

public class SetAbsorberAbsorptionLevelRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double AbsorptionLevelPercent { get; set; }
}
