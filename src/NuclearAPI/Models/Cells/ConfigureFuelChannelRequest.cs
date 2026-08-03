namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureFuelChannelRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double NeutronFlux { get; set; }
    public double LocalPowerOutputMW { get; set; }
    public string Status { get; set; }
}