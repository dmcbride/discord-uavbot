using uav.logic.Models;

namespace uav.logic.Database.Model;

public class PlanetInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Cost { get; set; }
    public GV CostGV => GV.FromString(Cost!);
    public string? Ore1 { get; set; }
    public int Ore1Yield { get; set; }
    public string? Ore2 { get; set; }
    public int Ore2Yield { get; set; }
    public string? Ore3 { get; set; }
    public int Ore3Yield { get; set; }
    public int Telescope { get; set; }
}