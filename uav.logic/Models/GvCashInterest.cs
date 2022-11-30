using System;

namespace uav.logic.Models;

public enum GvCashInterestVerb
{
    CashWindfall,
    GvArk,
    CashArk,
}

public interface IGvCashCompoundInterest
{
    public static IGvCashCompoundInterest FromVerb(GvCashInterestVerb verb) => verb switch
    {
        GvCashInterestVerb.CashWindfall => GvInterest.CashWindfall,
        GvCashInterestVerb.GvArk => GvInterest.Ark,
        GvCashInterestVerb.CashArk => CashInterest.Ark,
        _ => throw new ArgumentOutOfRangeException(nameof(verb), verb, "Unknown verb"),
    };

    public double rate { get; }

    public int CountUntilGoalGv(GvCash initial, GV goalGv);

    public GvCash CompoundMany(GvCash initial, int numPeriods);

    public int CountUntilMultiplier(double goalMultiplier)
    {
        if (goalMultiplier <= 1.0)
        {
            return 0;
        }
        var decimalCount = Math.Log(goalMultiplier, 1 + rate);
        return Convert.ToInt32(Math.Ceiling(decimalCount));
    }

    public double CalculateCompoundRate(int numPeriods)
    {
        if (numPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numPeriods), $"Number of periods {numPeriods} cannot be negative");
        }
        return Math.Pow(1.0 + rate, numPeriods);
    }
}

public readonly record struct CashInterest(double rate) : IGvCashCompoundInterest
{
    public static readonly CashInterest Ark = new(0.0475);

    public int CountUntilGoalGv(GvCash initial, GV goalGv)
    {
        if (initial.gv >= goalGv)
        {
            return 0;
        }
        var goalCash = initial.cash + (goalGv - initial.gv);
        var goalPercent = goalCash / initial.cash;
        return (this as IGvCashCompoundInterest).CountUntilMultiplier(goalPercent);
    }

    public GvCash CompoundMany(GvCash initial, int numPeriods)
    {
        var compoundRate = (this as IGvCashCompoundInterest).CalculateCompoundRate(numPeriods);
        var newCash = initial.cash * compoundRate;
        var newGv = initial.gv + (newCash - initial.cash);
        return new GvCash(newGv, newCash);
    }
}

public record class GvInterest(double rate) : IGvCashCompoundInterest
{
    public static readonly GvInterest Ark = new(0.02295);
    public static readonly GvInterest CashWindfall = new(0.1);

    public int CountUntilGoalGv(GvCash initial, GV goalGv)
    {
        if (initial.gv >= goalGv)
        {
            return 0;
        }
        var goalPercent = goalGv / initial.gv;
        return (this as IGvCashCompoundInterest).CountUntilMultiplier(goalPercent);
    }

    public GvCash CompoundMany(GvCash initial, int numPeriods)
    {
        var compoundRate = (this as IGvCashCompoundInterest).CalculateCompoundRate(numPeriods);
        var newGv = initial.gv * compoundRate;
        var newCash = initial.cash + (newGv - initial.gv);
        return new GvCash(newGv, newCash);
    }
}