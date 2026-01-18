using System;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Gv;

public abstract class BaseGvSlashCommand : BaseSlashCommand
{
    protected const double cashArkChance = 70d / 101d;
    protected const double dmArkChance = 1 - cashArkChance;
    protected const double arksPerHour = 10d;
    protected (double items, GV newValue) ArkCalculate(double gv, double goalGv, double cash, double exponent)
    {
        var multiplier = (goalGv - (gv - cash)) / cash;

        var arks = Math.Ceiling(Math.Log(multiplier) / Math.Log(exponent));
        var newValue = cash * Math.Pow(exponent, arks) + (gv - cash);

        return (arks, GV.FromNumber(newValue));
    }

}