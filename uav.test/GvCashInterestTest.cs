using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Models;

namespace uav.test;

[TestClass]
public class GvCashInterestTest
{
    public double multiplierTolerance = 1e-9;
    public double gvTolerance = 0.005;

    public void AssertIsGreater(double first, double second, double tolerance)
    {
        Assert.IsTrue(first > second - tolerance, $"{first} was not greater than ${second} within a tolerance of {tolerance}.");
    }

    [DataTestMethod]
    [DataRow(GvCashInterestVerb.CashWindfall, 0, 1.0)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1, 1.1)]
    [DataRow(GvCashInterestVerb.CashWindfall, 2, 1.21)]
    [DataRow(GvCashInterestVerb.CashWindfall, 3, 1.331)]
    [DataRow(GvCashInterestVerb.CashWindfall, 25, 10.835)]
    [DataRow(GvCashInterestVerb.CashWindfall, 49, 106.72)]
    [DataRow(GvCashInterestVerb.CashWindfall, 73, 1051.15)]
    public void CalculateCompoundRate(GvCashInterestVerb verb, int numPeriods, double expected)
    {
        var interest = IGvCashCompoundInterest.FromVerb(verb);
        var actual = interest.CalculateCompoundRate(numPeriods);
        Assert.AreEqual(expected, actual, gvTolerance);
    }

    [DataTestMethod]
    [DataRow(GvCashInterestVerb.CashWindfall, 0.5, 0)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1.0, 0)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1.1, 1)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1.21, 2)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1.331, 3)]
    [DataRow(GvCashInterestVerb.CashArk, 4.0, 30)]
    [DataRow(GvCashInterestVerb.CashArk, 10.0, 50)]
    [DataRow(GvCashInterestVerb.CashArk, 100.0, 100)]
    [DataRow(GvCashInterestVerb.CashArk, 1000.0, 149)]
    [DataRow(GvCashInterestVerb.GvArk, 1.25, 10)]
    [DataRow(GvCashInterestVerb.GvArk, 1.975, 30)]
    // Technically, 31 consecutive GV arks are impossible since crossover happens after 30.
    // But we're just testing the compound interest rate in this test, not crossover logic.
    [DataRow(GvCashInterestVerb.GvArk, 1.976, 31)]
    [DataRow(GvCashInterestVerb.CashWindfall, 4.0, 15)]
    [DataRow(GvCashInterestVerb.CashWindfall, 10.0, 25)]
    [DataRow(GvCashInterestVerb.CashWindfall, 100.0, 49)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 73)]
    public void CountUntilMultiplier(GvCashInterestVerb verb, double multiplier, int expected)
    {
        var interest = IGvCashCompoundInterest.FromVerb(verb);

        // Check our expected counts
        var actual = interest.CountUntilMultiplier(multiplier);
        Assert.AreEqual(expected, actual, $"Unexpected count to {verb} until multiplier {multiplier}");

        // Check our goal condition
        var initial = GV.FromNumber(1.0);
        var final = initial * interest.CalculateCompoundRate(expected);
        AssertIsGreater(final, multiplier, multiplierTolerance);

        // Check that our goal is the least possible count
        if (expected > 0)
        {
            var penultimate = initial * interest.CalculateCompoundRate(expected - 1);
            AssertIsGreater(multiplier, penultimate, multiplierTolerance);
        }
    }

    [DataTestMethod]
    [DataRow(GvCashInterestVerb.CashArk, 2000.0, 1000.0, 1500.0, 0)]
    [DataRow(GvCashInterestVerb.CashArk, 2000.0, 1000.0, 2000.0, 0)]
    [DataRow(GvCashInterestVerb.CashArk, 2000.0, 1000.0, 2047.5, 1)]
    [DataRow(GvCashInterestVerb.CashArk, 2000.0, 1000.0, 2047.6, 2)]
    [DataRow(GvCashInterestVerb.CashArk, 2000.0, 1000.0, 3000.0, 15)]
    [DataRow(GvCashInterestVerb.GvArk, 1000.0, 100.0, 500.0, 0)]
    [DataRow(GvCashInterestVerb.GvArk, 1000.0, 100.0, 1000.0, 0)]
    [DataRow(GvCashInterestVerb.GvArk, 1000.0, 100.0, 1022.95, 1)]
    [DataRow(GvCashInterestVerb.GvArk, 1000.0, 100.0, 1022.96, 2)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 0.0, 500.0, 0)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 0.0, 1000.0, 0)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 0.0, 1100.0, 1)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 0.0, 1100.1, 2)]
    [DataRow(GvCashInterestVerb.CashWindfall, 1000.0, 0.0, 10000.0, 25)]
    public void CountUntilGoalGv(GvCashInterestVerb verb, double initialGv, double initialCash, double goal, int expected)
    {
        var interest = IGvCashCompoundInterest.FromVerb(verb);
        var initial = GvCash.FromNumbers(initialGv, initialCash);
        var goalGv = GV.FromNumber(goal);

        // Check our expected counts
        int actual = interest.CountUntilGoalGv(initial, goalGv);
        Assert.AreEqual(expected, actual, $"Unexpected count to {verb} from {initial} to goal GV {goalGv}");

        // Check our goal condition
        var final = interest.CompoundMany(initial, expected);
        AssertIsGreater(final.gv, goalGv, gvTolerance);

        // Check that our goal is the least possible count
        if (expected > 0)
        {
            var penultimate = interest.CompoundMany(initial, expected - 1);
            AssertIsGreater(goalGv, penultimate.gv, gvTolerance);
        }
    }
}