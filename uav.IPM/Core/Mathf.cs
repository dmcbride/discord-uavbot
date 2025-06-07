namespace uav.IPM.Core;

// hack to get the IPM code (which is in Unity) to compile in .NET
internal class Mathf
{
    internal static double FloorToInt(double v)
    {
        return (int)Math.Floor(v);
    }

    internal static int RoundToInt(double v)
    {
        return (int)Math.Round(v, MidpointRounding.ToEven);
    }

    internal static double Log10(double v)
    {
        return Math.Log10(v);
    }

    internal static double Pow(double v, double p)
    {
        return Math.Pow(v, p);
    }
}