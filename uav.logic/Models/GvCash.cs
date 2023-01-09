using System;

namespace uav.logic.Models;

// A pair that represents a galaxy's total value and also the cash on hand
public class GvCash
{
    public readonly GV gv;
    public readonly GV cash;

    public GvCash(GV gv, GV cash)
    {
        if (gv < cash)
        {
            throw new ArgumentException($"Your cash on hand ({cash}) is more than your current GV ({gv}), that's probably wrong.");
        }
        if (gv <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(gv), $"Your current GV ({gv}) cannot be zero, it must be higher.");
        }
        this.gv = gv;
        this.cash = cash;
    }

    public static GvCash FromNumbers(double gv, double cash)
    {
        return new GvCash(GV.FromNumber(gv), GV.FromNumber(cash));
    }

    public static bool TryFromStrings(string gv, string cash, out GvCash gvCash, out string error)
    {
        gvCash = null;

        if (!GV.TryFromString(gv, out var gvValue, out error) ||
            !GV.TryFromString(cash, out var cashValue, out error))
        {
            return false;
        }

        try
        {
            gvCash = new GvCash(gvValue, cashValue);
            return true;
        }
        catch (ArgumentException e)
        {
            error = e.Message;
            return false;
        }
    }

    public override string ToString()
    {
        return $"(GV {gv}, Cash {cash})";
    }

    public double CashPercent()
    {
        return cash / gv;
    }

    public bool IsBeyondPercent(double goalPercent)
    {
        return gv * goalPercent <= cash;
    }

    public bool IsBeyondCrossover()
    {
        return IsBeyondPercent(ArkCrossoverPercent);
    }

    public int CountArksUntilCrossover()
    {
        return CountUntilCashPercent(ArkCrossoverPercent, GvInterest.Ark);
    }

    public GvCash PlusIncome(GV income)
    {
        return new GvCash(gv + income, cash + income);
    }

    public GvCash ArkUntilGv(GV goalGv)
    {
        return ArkUntilGv(goalGv, out _, out _);
    }

    public GvCash ArkUntilGv(GV goalGv, out int totalArks, out int arksBeforeCrossover)
    {
        // Has the goal already been reached?
        if (goalGv <= gv)
        {
            totalArks = 0;
            arksBeforeCrossover = 0;
            return this;
        }

        // First calculate cash percent upon achieving the goal
        var goalPercent = PlusIncome(goalGv - gv).CashPercent();

        // Ark until crossover happens or goal is reached
        arksBeforeCrossover = CountUntilCashPercent(Math.Min(goalPercent, ArkCrossoverPercent), GvInterest.Ark);
        var crossoverGvCash = GvInterest.Ark.CompoundMany(this, arksBeforeCrossover);

        // Now ark until goal is reached (if not already reached)
        var cashArkCount = CashInterest.Ark.CountUntilGoalGv(crossoverGvCash, goalGv);
        totalArks = arksBeforeCrossover + cashArkCount;
        return CashInterest.Ark.CompoundMany(crossoverGvCash, cashArkCount);
    }

    public GvCash ArkMany(int numPeriods)
    {
        var gvArkCount = Math.Min(numPeriods, CountArksUntilCrossover());
        var crossoverGvCash = GvInterest.Ark.CompoundMany(this, gvArkCount);

        var cashArkCount = numPeriods - gvArkCount;
        return CashInterest.Ark.CompoundMany(crossoverGvCash, cashArkCount);
    }

    public GvCash CashWindfallUntilGv(GV goal)
    {
        return CashWindfallUntilGv(goal, out _);
    }

    public GvCash CashWindfallMany(int numPeriods)
    {
        return GvInterest.CashWindfall.CompoundMany(this, numPeriods);
    }

    public GvCash CashWindfallUntilGv(GV goal, out int totalCws)
    {
        // Has the goal already been reached?
        if (goal <= gv)
        {
            totalCws = 0;
            return this;
        }

        totalCws = GvInterest.CashWindfall.CountUntilGoalGv(this, goal);
        return GvInterest.CashWindfall.CompoundMany(this, totalCws);
    }

    // The goal is to find the smallest non-negative count of income iterations, until the following is satisfied:
    //
    //      goalPercent <= newCash(count) / newGv(count)
    //
    // where newGv(...) and newCash(...) are determined as:
    //
    //      newGv(count) := gv * Math.pow(1 + interest.rate, count)
    //      newCash(count) := cash + (newGv(count) - gv)
    //
    // Assumes 0 <= cash <= gv
    // Assumes 0 <= goalPercent <= 1
    // Assumes 0 < gvIncomeFactor
    //
    // For practical purposes, gvIncomeFactor can either be gvArkFactor or cashWindfallFactor
    private int CountUntilCashPercent(double goalPercent, GvInterest interest)
    {
        // Check if the crossover already has happened without any new income
        if (IsBeyondPercent(goalPercent))
        {
            return 0;
        }

        // We can mathematically derive an efficient log-based formula.
        //
        // We can expand newCash(count) in terms of newGv(count) and also mathematically rearrange the crossover inequality:
        //
        //      goalPercent <= newCash(count) / newGv(count)
        //      newGv(count) * goalPercent <= newCash(count)
        //      newGv(count) * goalPercent <= cash + (newGv(count) - gv)
        //      -cash + gv <= newGv(count) - newGv(count) * goalPercent
        //      gv - cash <= newGv(count) * (1 - goalPercent)
        //      (gv - cash) / (1 - goalPercent) <= newGv(count)
        //
        // Next, we can expand newGv(count) in terms of the count we seek and also further rearrange:
        //
        //      (gv - cash) / (1 - goalPercent) <= gv * Math.pow(1 + interest.rate, count)
        //      (1 - cash / gv) / (1 - goalPercent) <= Math.pow(1 + interest.rate, count)
        //
        // At last, we can take logarithms to isolate the count on one side.
        //
        //      Math.Log((1 - cash / gv) / (1 - goalPercent), 1 + interest.rate) <= count
        //
        // The smallest integer is now solvable with a Math.Ceiling on the left hand side.

        var cashGapPercent = 1 - CashPercent();
        var goalGapPercent = 1 - goalPercent;

        var goalIncomeMultiplier = cashGapPercent / goalGapPercent;
        return (interest as IGvCashCompoundInterest).CountUntilMultiplier(goalIncomeMultiplier);
    }

    private static readonly double ArkCrossoverPercent = GvInterest.Ark.rate / CashInterest.Ark.rate;
}
