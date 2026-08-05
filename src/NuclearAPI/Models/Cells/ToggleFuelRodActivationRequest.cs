namespace Nuclear_Reactor_Sim.Models.Cells;

public class ToggleFuelRodActivationRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}