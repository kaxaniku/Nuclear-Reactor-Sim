namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureCoolerRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double WaterFlowRate { get; set; }
    public double CoolantLevelPercent { get; set; }
}
