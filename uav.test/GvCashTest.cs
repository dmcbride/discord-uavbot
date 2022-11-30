using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Models;

namespace uav.test;

[TestClass]
public class GvCashTest
{
    public double percentTolerance = 1e-9;
    public double gvTolerance = 0.005;

    public void AssertIsGreater(GV first, GV second)
    {
        Assert.IsTrue(first > second - gvTolerance, $"{first} was not greater than ${second} within a tolerance of {gvTolerance}.");
    }

    [TestMethod]
    public void TryFromStrings()
    {
        Assert.IsTrue(GvCash.TryFromStrings("98.76k", "54.32k", out var gvCash, out var error));
        Assert.IsNull(error);
        Assert.AreEqual(98760.0, gvCash.gv);
        Assert.AreEqual(54320.0, gvCash.cash);
    }

    [DataTestMethod]
    [DataRow("0", "0", "current GV .* cannot be zero")]
    [DataRow("100s", "10S", "cash .* more than .* current GV .* wrong")]
    public void TryFromStringsErrors(string gv, string cash, string errorRegex)
    {
        Assert.IsFalse(GvCash.TryFromStrings(gv, cash, out _, out var error));
        StringAssert.Matches(error, new Regex(errorRegex));
    }

    [TestMethod]
    public void ToStringGvCash()
    {
        var gvCash = GvCash.FromNumbers(987, 654);
        StringAssert.Matches(gvCash.ToString(), new Regex("GV.*987.*Cash.*654"));
    }

    [DataTestMethod]
    [DataRow(100.0, 0.0, 0.0, false)]
    [DataRow(100.0, 33.3, 0.333, false)]
    [DataRow(100.0, 100.0, 1.0, true)]
    [DataRow(2000.0, 500.0, 0.25, false)]
    [DataRow(2468.0, 1234.0, 0.5, true)]
    public void CashPercent(double gv, double cash, double expected, bool expectedBeyondCrossover)
    {
        var gvCash = GvCash.FromNumbers(gv, cash);
        var actual = gvCash.CashPercent();
        Assert.AreEqual(expected, actual, percentTolerance);
        Assert.IsTrue(gvCash.IsBeyondPercent(expected - percentTolerance));
        Assert.IsFalse(gvCash.IsBeyondPercent(expected + percentTolerance));
        Assert.AreEqual(expectedBeyondCrossover, gvCash.IsBeyondCrossover());
    }

    [DataTestMethod]
    [DataRow(1000.0, 0.0, 30)]
    [DataRow(1000.0, 1.0, 30)]
    [DataRow(1000.0, 10.0, 29)]
    [DataRow(1000.0, 100.0, 25)]
    [DataRow(1000.0, 400.0, 7)]
    [DataRow(1000.0, 483.0, 1)]
    [DataRow(1000.0, 484.0, 0)]
    [DataRow(1000.0, 1000.0, 0)]
    public void CountArksUntilCrossover(double gv, double cash, int expected)
    {
        var gvCash = GvCash.FromNumbers(gv, cash);

        // Check our expected counts
        var actual = gvCash.CountArksUntilCrossover();
        Assert.AreEqual(expected, actual);

        // Check our goal condition
        var final = gvCash.ArkMany(expected);
        Assert.IsTrue(final.IsBeyondCrossover());

        // Check that our goal is the least possible count
        if (expected > 0)
        {
            var penultimate = gvCash.ArkMany(expected - 1);
            Assert.IsFalse(penultimate.IsBeyondCrossover());
        }
    }

    [TestMethod]
    public void PlusIncome()
    {
        var initial = GvCash.FromNumbers(123.4, 56.7);
        var actual = initial.PlusIncome(GV.FromNumber(800.0));
        var expected = GvCash.FromNumbers(923.4, 856.7);
        Assert.AreEqual(expected.gv, actual.gv, gvTolerance);
        Assert.AreEqual(expected.cash, actual.cash, gvTolerance);
    }

    [DataTestMethod]
    // When cash is 0.0, the expected new cash is: gv * (1.02295^numGvArks - 1) * 1.0475^numCashArks
    // At 30 or less arks when cash starts at 0, expected new cash simplifies to: gv * (1.02295^numGvArks - 1)
    [DataRow(1000.0, 0.0, 500.0, 0, 0, 1000.0, 0.0)]
    [DataRow(1000.0, 0.0, 1000.0, 0, 0, 1000.0, 0.0)]
    [DataRow(1000.0, 0.0, 1001.0, 1, 1, 1022.95, 22.95)]
    [DataRow(1000.0, 0.0, 1022.0, 1, 1, 1022.95, 22.95)]
    [DataRow(1000.0, 0.0, 1023.0, 2, 2, 1046.43, 46.43)]
    [DataRow(1000.0, 0.0, 1934.0, 30, 30, 1975.3, 975.3)]
    // Over 30 arks when cash starts at 0, expected new cash simplifies to: gv * (1.02295^30 - 1) * 1.0475^numCashArks
    [DataRow(1000.0, 0.0, 1976.0, 31, 30, 2021.62, 1021.62)]
    [DataRow(1000.0, 0.0, 10000.0, 78, 30, 10047.53, 9047.53)]
    [DataRow(1000.0, 0.0, 100000.0, 130, 30, 102050.77, 101050.77)]
    // In general, the expected new cash is: (cash + gv * (1.02295^numGvArks - 1)) * 1.0475^numCashArks
    [DataRow(1000.0, 477.05, 1020.0, 1, 1, 1022.95, 500.0)]
    [DataRow(1000.0, 477.05, 1040.0, 2, 1, 1046.7, 523.75)]
    // Here are expectations for cash arking for various cashes on hand.
    [DataRow(1000.0, 100.0, 2000.0, 31, 25, 2040.66, 1140.66)]
    [DataRow(1000.0, 200.0, 2000.0, 30, 20, 2031.55, 1231.55)]
    [DataRow(1000.0, 300.0, 2000.0, 29, 14, 2051.82, 1351.82)]
    [DataRow(1000.0, 400.0, 2000.0, 27, 7, 2047.39, 1447.39)]
    [DataRow(1000.0, 500.0, 2000.0, 24, 0, 2022.88, 1522.88)]
    [DataRow(1000.0, 900.0, 2000.0, 17, 0, 2080.89, 1980.89)]
    [DataRow(1000.0, 1000.0, 2000.0, 15, 0, 2005.91, 2005.91)]
    public void ArkUntilGv(double initialGv, double initialCash, double goal, int expectedTotalArks, int expectedArksBeforeCrossover, double expectedGv, double expectedCash)
    {
        var initial = GvCash.FromNumbers(initialGv, initialCash);
        var goalGv = GV.FromNumber(goal);

        // Check our expected counts
        var actual = initial.ArkUntilGv(goalGv, out var totalArks, out var arksBeforeCrossover);
        Assert.AreEqual(expectedTotalArks, totalArks);
        Assert.AreEqual(expectedArksBeforeCrossover, arksBeforeCrossover);
        Assert.AreEqual(expectedGv, actual.gv, gvTolerance);
        Assert.AreEqual(expectedCash, actual.cash, gvTolerance);

        // Check our goal condition
        AssertIsGreater(actual.gv, goalGv);
        var final = initial.ArkMany(expectedTotalArks);
        Assert.AreEqual(expectedGv, final.gv, gvTolerance);
        Assert.AreEqual(expectedCash, final.cash, gvTolerance);

        // Check that our goal is the least possible count
        if (expectedTotalArks > 0)
        {
            var penultimate = initial.ArkMany(expectedTotalArks - 1);
            AssertIsGreater(goalGv, penultimate.gv);
        }
    }

    [DataTestMethod]
    [DataRow(1000.0, 0.0, 500.0, 0, 1000.0, 0.0)]
    [DataRow(1000.0, 0.0, 1000.0, 0, 1000.0, 0.0)]
    [DataRow(1000.0, 0.0, 1001.0, 1, 1100.0, 100.0)]
    [DataRow(1000.0, 0.0, 1100.0, 1, 1100.0, 100.0)]
    [DataRow(1000.0, 0.0, 1101.0, 2, 1210.0, 210.0)]
    [DataRow(1000.0, 500.0, 1101.0, 2, 1210.0, 710.0)]
    [DataRow(1000.0, 1000.0, 1101.0, 2, 1210.0, 1210.0)]
    [DataRow(1000.0, 100.0, 10000.0, 25, 10834.71, 9934.71)]
    public void CashWindfallUntilGv(double initialGv, double initialCash, double goal, int expectedTotalCws, double expectedGv, double expectedCash)
    {
        var initial = GvCash.FromNumbers(initialGv, initialCash);
        var goalGv = GV.FromNumber(goal);

        // Check our expected counts
        var actual = initial.CashWindfallUntilGv(goalGv, out var totalCws);
        Assert.AreEqual(expectedTotalCws, totalCws);
        Assert.AreEqual(expectedGv, actual.gv, gvTolerance);
        Assert.AreEqual(expectedCash, actual.cash, gvTolerance);

        // Check our goal condition
        AssertIsGreater(actual.gv, goalGv);
        var final = initial.CashWindfallMany(expectedTotalCws);
        Assert.AreEqual(expectedGv, final.gv, gvTolerance);
        Assert.AreEqual(expectedCash, final.cash, gvTolerance);

        // Check that our goal is the least possible count
        if (expectedTotalCws > 0)
        {
            var penultimate = initial.CashWindfallMany(expectedTotalCws - 1);
            AssertIsGreater(goalGv, penultimate.gv);
        }
    }
}