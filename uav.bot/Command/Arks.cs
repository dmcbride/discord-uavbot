using System;
using uav.logic.Models;

namespace uav.Command;

public class Arks : CommandBase
{
    public (double items, GV newValue) ArkCalculate(double gv, double goalGv, double cash, double exponent)
    {
        var multiplier = (goalGv - (gv - cash)) / cash;

        var arks = Math.Ceiling(Math.Log(multiplier) / Math.Log(exponent));
        var newValue = cash * Math.Pow(exponent, arks) + (gv - cash);

        return (arks, GV.FromNumber(newValue));
    }
}
