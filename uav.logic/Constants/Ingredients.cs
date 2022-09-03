using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uav.logic.Extensions;
using uav.logic.Models;

namespace uav.logic.Constants;

public enum Ingredient
{
    // ores
    Copper,
    Iron,
    Lead,
    Silica,
    Aluminum,
    Silver,
    Gold,
    Diamond,
    Platinum,
    Titanium,
    Iridium,
    Palladium,
    Osmium,
    Rhodium,
    Inerton,
    Quadium,
    Scrith,
    Uru,
    Vibranium,
    Aether,
    Viterium,
    Xynium,
    Quolium,
    Luterium,
    Wraith,

    // alloys
    [UnlockCostAttribute("0")]
    [BaseValue("$1.45E+03")]
    [BuildTime("00:00:20")]
    [Ingredient(Ingredient.Copper,1000)]
    CopperBar,

    [UnlockCostAttribute("3k")]
    [BaseValue("$3.00E+03")]
    [BuildTime("00:00:30")]
    [Ingredient(Ingredient.Iron,1000)]
    IronBar,

    [UnlockCostAttribute("9k")]
    [BaseValue("$6.10E+03")]
    [BuildTime("00:00:40")]
    [Ingredient(Ingredient.Lead,1000)]
    LeadBar,

    [UnlockCostAttribute("25k")]
    [BaseValue("$1.25E+04")]
    [BuildTime("00:01:00")]
    [Ingredient(Ingredient.Silica,1000)]
    SiliconBar,

    [UnlockCostAttribute("75k")]
    [BaseValue("$2.76E+04")]
    [BuildTime("00:01:20")]
    [Ingredient(Ingredient.Aluminum,1000)]
    AluminumBar,

    [UnlockCostAttribute("225k")]
    [BaseValue("$6.00E+04")]
    [BuildTime("00:02:00")]
    [Ingredient(Ingredient.Silver,1000)]
    SilverBar,

    [UnlockCostAttribute("500k")]
    [BaseValue("$1.20E+05")]
    [BuildTime("00:03:00")]
    [Ingredient(Ingredient.Gold,1000)]
    GoldBar,

    [UnlockCostAttribute("1m")]
    [BaseValue("$2.34E+05")]
    [BuildTime("00:04:00")]
    [Ingredient(Ingredient.CopperBar,10)]
    [Ingredient(Ingredient.SilverBar,2)]
    BronzeAlloy,

    [UnlockCostAttribute("2m")]
    [BaseValue("$3.40E+05")]
    [BuildTime("00:08:00")]
    [Ingredient(Ingredient.IronBar,30)]
    [Ingredient(Ingredient.LeadBar,15)]
    SteelAlloy,

    [UnlockCostAttribute("4m")]
    [BaseValue("$7.80E+05")]
    [BuildTime("00:10:00")]
    [Ingredient(Ingredient.Platinum,1000)]
    [Ingredient(Ingredient.GoldBar,2)]
    PlatinumAlloy,

    [UnlockCostAttribute("8m")]
    [BaseValue("$1.63E+06")]
    [BuildTime("00:12:00")]
    [Ingredient(Ingredient.Titanium,1000)]
    [Ingredient(Ingredient.BronzeAlloy,2)]
    TitaniumAlloy,

    [UnlockCostAttribute("15m")]
    [BaseValue("$3.11E+06")]
    [BuildTime("00:14:00")]
    [Ingredient(Ingredient.Iridium,1000)]
    [Ingredient(Ingredient.SteelAlloy,2)]
    IridiumAlloy,

    [UnlockCostAttribute("30m")]
    [BaseValue("$7.00E+06")]
    [BuildTime("00:16:00")]
    [Ingredient(Ingredient.Palladium,1000)]
    [Ingredient(Ingredient.PlatinumAlloy,2)]
    PalladiumAlloy,

    [UnlockCostAttribute("60m")]
    [BaseValue("$1.45E+07")]
    [BuildTime("00:18:00")]
    [Ingredient(Ingredient.Osmium,1000)]
    [Ingredient(Ingredient.TitaniumAlloy,2)]
    OsmiumAlloy,

    [UnlockCostAttribute("120m")]
    [BaseValue("$3.10E+07")]
    [BuildTime("00:20:00")]
    [Ingredient(Ingredient.Rhodium,1000)]
    [Ingredient(Ingredient.IridiumAlloy,2)]
    RhodiumAlloy,

    [UnlockCostAttribute("250m")]
    [BaseValue("$6.80E+07")]
    [BuildTime("00:24:00")]
    [Ingredient(Ingredient.Inerton,1000)]
    [Ingredient(Ingredient.PalladiumAlloy,2)]
    InertonAlloy,

    [UnlockCostAttribute("500m")]
    [BaseValue("$1.52E+08")]
    [BuildTime("00:28:00")]
    [Ingredient(Ingredient.Quadium,1000)]
    [Ingredient(Ingredient.OsmiumAlloy,2)]
    QuadiumAlloy,

    [UnlockCostAttribute("1B")]
    [BaseValue("$3.52E+08")]
    [BuildTime("00:32:00")]
    [Ingredient(Ingredient.Scrith,1000)]
    [Ingredient(Ingredient.RhodiumAlloy,2)]
    ScrithAlloy,

    [UnlockCostAttribute("2B")]
    [BaseValue("$8.32E+08")]
    [BuildTime("00:36:00")]
    [Ingredient(Ingredient.Uru,1000)]
    [Ingredient(Ingredient.InertonAlloy,2)]
    UruAlloy,

    [UnlockCostAttribute("4B")]
    [BaseValue("$2.05E+09")]
    [BuildTime("00:40:00")]
    [Ingredient(Ingredient.Vibranium,1000)]
    [Ingredient(Ingredient.QuadiumAlloy,2)]
    VibraniumAlloy,

    [UnlockCostAttribute("8B")]
    [BaseValue("$5.12E+09")]
    [BuildTime("00:44:00")]
    [Ingredient(Ingredient.Aether,1000)]
    [Ingredient(Ingredient.ScrithAlloy,2)]
    AetherAlloy,

    [UnlockCostAttribute("16B")]
    [BaseValue("$1.50E+10")]
    [BuildTime("00:48:00")]
    [Ingredient(Ingredient.Viterium,1000)]
    [Ingredient(Ingredient.UruAlloy,2)]
    ViteriumAlloy,

    [UnlockCostAttribute("84B")]
    [BaseValue("$4.80E+10")]
    [BuildTime("00:55:00")]
    [Ingredient(Ingredient.Xynium,1500)]
    [Ingredient(Ingredient.VibraniumAlloy,5)]
    XyniumAlloy,

    [UnlockCostAttribute("275B")]
    [BaseValue("$1.60E+11")]
    [BuildTime("01:02:00")]
    [Ingredient(Ingredient.Quolium,1500)]
    [Ingredient(Ingredient.AetherAlloy,5)]
    QuoliumAlloy,

    [UnlockCostAttribute("1T")]
    [BaseValue("$6.00E+11")]
    [BuildTime("01:09:00")]
    [Ingredient(Ingredient.Luterium,1500)]
    [Ingredient(Ingredient.ViteriumAlloy,5)]
    LuteriumAlloy,

    [UnlockCostAttribute("4.2T")]
    [BaseValue("$2.40E+12")]
    [BuildTime("01:16:00")]
    [Ingredient(Ingredient.Wraith,1500)]
    [Ingredient(Ingredient.XyniumAlloy,5)]
    WraithAlloy,


    // items
    [UnlockCostAttribute("0")]
    [BaseValue("10k")]
    [BuildTime("00:01:00")]
    [Ingredient(Ingredient.CopperBar,5)]
    CopperWire,

    [UnlockCostAttribute("20k")]
    [BaseValue("20k")]
    [BuildTime("00:02:00")]
    [Ingredient(Ingredient.IronBar,5)]
    IronNails,

    [UnlockCostAttribute("50k")]
    [BaseValue("70k")]
    [BuildTime("00:04:00")]
    [Ingredient(Ingredient.CopperBar,10)]
    [Ingredient(Ingredient.CopperWire,2)]
    Battery,

    [UnlockCostAttribute("100k")]
    [BaseValue("135k")]
    [BuildTime("00:08:00")]
    [Ingredient(Ingredient.LeadBar,5)]
    [Ingredient(Ingredient.IronNails,2)]
    Hammer,

    [UnlockCostAttribute("200k")]
    [BaseValue("220k")]
    [BuildTime("00:12:00")]
    [Ingredient(Ingredient.SiliconBar,10)]
    Glass,

    [UnlockCostAttribute("400k")]
    [BaseValue("620k")]
    [BuildTime("00:20:00")]
    [Ingredient(Ingredient.SiliconBar,5)]
    [Ingredient(Ingredient.AluminumBar,5)]
    [Ingredient(Ingredient.CopperWire,10)]
    Circuit,

    [UnlockCostAttribute("1M")]
    [BaseValue("1.1M")]
    [BuildTime("00:40:00")]
    [Ingredient(Ingredient.SilverBar,5)]
    [Ingredient(Ingredient.Glass,1)]
    Lens,

    [UnlockCostAttribute("2M")]
    [BaseValue("3.2M")]
    [BuildTime("01:00:00")]
    [Ingredient(Ingredient.IronBar,10)]
    [Ingredient(Ingredient.GoldBar,5)]
    [Ingredient(Ingredient.Lens,1)]
    Laser,

    [UnlockCostAttribute("5M")]
    [BaseValue("7.6M")]
    [BuildTime("01:20:00")]
    [Ingredient(Ingredient.SilverBar,5)]
    [Ingredient(Ingredient.Circuit,5)]
    BasicComputer,

    [UnlockCostAttribute("10M")]
    [BaseValue("12.5M")]
    [BuildTime("01:40:00")]
    [Ingredient(Ingredient.Glass,10)]
    [Ingredient(Ingredient.Circuit,5)]
    SolarPanel,

    [UnlockCostAttribute("20M")]
    [BaseValue("31M")]
    [BuildTime("02:00:00")]
    [Ingredient(Ingredient.BronzeAlloy,5)]
    [Ingredient(Ingredient.Lens,5)]
    [Ingredient(Ingredient.Laser,2)]
    LaserTorch,

    [UnlockCostAttribute("30M")]
    [BaseValue("35M")]
    [BuildTime("02:30:00")]
    [Ingredient(Ingredient.Battery,30)]
    [Ingredient(Ingredient.SteelAlloy,20)]
    AdvancedBattery,

    [UnlockCostAttribute("50M")]
    [BaseValue("71.5M")]
    [BuildTime("03:00:00")]
    [Ingredient(Ingredient.PlatinumAlloy,5)]
    [Ingredient(Ingredient.Glass,5)]
    [Ingredient(Ingredient.Laser,2)]
    ThermalScanner,

    [UnlockCostAttribute("120M")]
    [BaseValue("180M")]
    [BuildTime("03:30:00")]
    [Ingredient(Ingredient.TitaniumAlloy,5)]
    [Ingredient(Ingredient.BasicComputer,5)]
    AdvancedComputer,

    [UnlockCostAttribute("250M")]
    [BaseValue("1B")]
    [BuildTime("03:45:00")]
    [Ingredient(Ingredient.LaserTorch,1)]
    [Ingredient(Ingredient.ThermalScanner,1)]
    NavigationModule,

    [UnlockCostAttribute("550M")]
    [BaseValue("1.1B")]
    [BuildTime("04:10:00")]
    [Ingredient(Ingredient.IridiumAlloy,15)]
    [Ingredient(Ingredient.LaserTorch,5)]
    PlasmaTorch,

    [UnlockCostAttribute("1.5B")]
    [BaseValue("1.145B")]
    [BuildTime("04:20:00")]
    [Ingredient(Ingredient.AluminumBar,150)]
    [Ingredient(Ingredient.PlatinumAlloy,75)]
    [Ingredient(Ingredient.TitaniumAlloy,50)]
    RadioTower,

    [UnlockCostAttribute("5B")]
    [BaseValue("2.7B")]
    [BuildTime("04:40:00")]
    [Ingredient(Ingredient.Lens,20)]
    [Ingredient(Ingredient.AdvancedComputer,1)]
    Telescope,

    [UnlockCostAttribute("17.5B")]
    [BaseValue("3.4B")]
    [BuildTime("05:00:00")]
    [Ingredient(Ingredient.SteelAlloy,150)]
    [Ingredient(Ingredient.PalladiumAlloy,30)]
    SatelliteDish,

    [UnlockCostAttribute("60B")]
    [BaseValue("7B")]
    [BuildTime("05:20:00")]
    [Ingredient(Ingredient.BronzeAlloy,500)]
    [Ingredient(Ingredient.Hammer,200)]
    Motor,

    [UnlockCostAttribute("100B")]
    [BaseValue("12B")]
    [BuildTime("05:40:00")]
    [Ingredient(Ingredient.OsmiumAlloy,20)]
    [Ingredient(Ingredient.AdvancedBattery,3)]
    Accumulator,

    [UnlockCostAttribute("200B")]
    [BaseValue("26B")]
    [BuildTime("05:50:00")]
    [Ingredient(Ingredient.RhodiumAlloy,5)]
    [Ingredient(Ingredient.PlasmaTorch,1)]
    NuclearCapsule,

    [UnlockCostAttribute("500B")]
    [BaseValue("140B")]
    [BuildTime("06:00:00")]
    [Ingredient(Ingredient.AluminumBar,300)]
    [Ingredient(Ingredient.Motor,1)]
    WindTurbine,

    [UnlockCostAttribute("1000B")]
    [BaseValue("180B")]
    [BuildTime("06:10:00")]
    [Ingredient(Ingredient.SolarPanel,25)]
    [Ingredient(Ingredient.Telescope,1)]
    [Ingredient(Ingredient.SatelliteDish,1)]
    SpaceProbe,

    [UnlockCostAttribute("2T")]
    [BaseValue("1T")]
    [BuildTime("06:20:00")]
    [Ingredient(Ingredient.IridiumAlloy,300)]
    [Ingredient(Ingredient.NuclearCapsule,1)]
    NuclearReactor,

    [UnlockCostAttribute("3T")]
    [BaseValue("2T")]
    [BuildTime("06:25:00")]
    [Ingredient(Ingredient.InertonAlloy,500)]
    [Ingredient(Ingredient.QuadiumAlloy,100)]
    Collider,

    [UnlockCostAttribute("6T")]
    [BaseValue("15T")]
    [BuildTime("06:45:00")]
    [Ingredient(Ingredient.AdvancedComputer,60)]
    [Ingredient(Ingredient.NuclearReactor,1)]
    GravitiyChamber,

    [UnlockCostAttribute("15T")]
    [BaseValue("70T")]
    [BuildTime("07:05:00")]
    [Ingredient(Ingredient.ScrithAlloy,300)]
    [Ingredient(Ingredient.Accumulator,90)]
    Robot,

    [UnlockCostAttribute("75T")]
    [BaseValue("240T")]
    [BuildTime("07:17:00")]
    [Ingredient(Ingredient.UruAlloy,200)]
    [Ingredient(Ingredient.VibraniumAlloy,100)]
    [Ingredient(Ingredient.NuclearCapsule,100)]
    FusionCapsule,

    [UnlockCostAttribute("500T")]
    [BaseValue("1.8q")]
    [BuildTime("07:30:00")]
    [Ingredient(Ingredient.NavigationModule,250)]
    [Ingredient(Ingredient.GravitiyChamber,1)]
    Teleporter,

    [UnlockCostAttribute("2.5q")]
    [BaseValue("40q")]
    [BuildTime("08:20:00")]
    [Ingredient(Ingredient.NuclearReactor,50)]
    [Ingredient(Ingredient.Collider,40)]
    [Ingredient(Ingredient.FusionCapsule,1)]
    FusionReactor,
}

public abstract class BaseGVAttr : Attribute
{
    protected GV gv { get; }
    public BaseGVAttr(string gv)
    {
        this.gv = Models.GV.FromString(gv);
    }

    public BaseGVAttr(double gv)
    {
        this.gv = Models.GV.FromNumber(gv);
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class UnlockCostAttribute : BaseGVAttr
{
    public GV Cost => gv;
    public UnlockCostAttribute(string gv) : base(gv)
    {
    }

    public UnlockCostAttribute(double gv) : base(gv)
    {
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class BaseValueAttribute : BaseGVAttr
{
    public GV Value => gv;
    public BaseValueAttribute(string gv) : base(gv)
    {
    }

    public BaseValueAttribute(double gv) : base(gv)
    {
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class BuildTimeAttribute : Attribute
{
    private uint craftTime { get; }
    public string BuildTime()
    {
        return BuildTime(craftTime);
    }

    private static string BuildTime(uint seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);

        var fmt = t.Hours > 0
            ? "h:mm:ss"
            : t.Minutes > 0
            ? "m:ss"
            : t.Seconds > 0
            ? "ss"
            : "0.ff";
        
        return t.ToString(fmt);
    }
    
    public BuildTimeAttribute(string time)
    {
        var t = TimeSpan.Parse(time);
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class IngredientAttribute : Attribute
{
    public Ingredient Input { get; }
    public int Quantity { get; }
    public IngredientAttribute(Ingredient input, int quantity)
    {
        Input = input;
        Quantity = quantity;
    }
}

public static class IngredientExtensions
{
    private static IEnumerable<T> Attributes<T>(Ingredient i) where T : Attribute
    {
        var type = typeof(Ingredient);
        var memInfo = type.GetMember(i.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Cast<T>();
    }

    public static GV UnlockCost(this Ingredient i)
    {
        var cost = Attributes<UnlockCostAttribute>(i).FirstOrDefault();
        return cost?.Cost ?? GV.Zero;
    }

    public static GV BaseValue(this Ingredient i)
    {
        var val = Attributes<BaseValueAttribute>(i).FirstOrDefault();
        return val?.Value ?? GV.Zero;
    }

    public static GV Value(this Ingredient i, int stars = 0, int salesLevel = 0, bool hasMerchantShip = false)
    {
        var salesBonus = salesLevel == 0 ? 1d : 1.1 + 0.05 * salesLevel;
        return GV.FromNumber(
            i.BaseValue() * (1 + stars / 5d) * salesBonus * (hasMerchantShip ? 2 : 1)
        );
    }

    public static string BuildTime(this Ingredient i)
    {
        var build = Attributes<BuildTimeAttribute>(i).FirstOrDefault();
        return build?.BuildTime() ?? string.Empty;
    }

    public static ICollection<(Ingredient ingredient, int quantity)> Ingredients(this Ingredient i, int multiple = 1)
    {
        var ingredients = Attributes<IngredientAttribute>(i)
            .OrderBy(x => x.Input)
            .Select(x => (ingredient: x.Input, quantity: x.Quantity * multiple))
            .ToArray();
        return ingredients;
    }

    public static ICollection<(Ingredient ingredient, int quantity)> IngredientsRecursive(this Ingredient i, int multiple = 1)
    {
        var ingredients = Attributes<IngredientAttribute>(i)
            .OrderBy(x => x.Input)
            .SelectMany(x => (ingredient: x.Input, quantity: x.Quantity * multiple).AndThen(x.Input.IngredientsRecursive(x.Quantity)))
            .ToArray();
        return ingredients;
    }
}