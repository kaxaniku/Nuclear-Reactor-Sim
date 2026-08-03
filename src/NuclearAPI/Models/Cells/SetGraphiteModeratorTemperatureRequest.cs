namespace Nuclear_Reactor_Sim.Models.Cells;

public class SetGraphiteModeratorTemperatureRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double TemperatureCelsius { get; set; }
}
