using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Models;

namespace uav.test
{
    [TestClass]
    public class ArkMathServiceTest
    {
        [DataTestMethod]
        [DataRow(1000, 0, 30)]
        [DataRow(1000, 1, 30)]
        [DataRow(1000, 10, 29)]
        [DataRow(1000, 100, 25)]
        [DataRow(1000, 400, 7)]
        [DataRow(1000, 483, 1)]
        [DataRow(1000, 484, 0)]
        [DataRow(1000, 1000, 0)]
        public void CountArksUntilCrossover(double gv, double cash, int expected)
        {
            // Check our expected counts
            var arkMath = new ArkMathService();
            var actual = arkMath.CountUntilCrossover(gv, cash);
            Assert.AreEqual(expected, actual);

            // Check our goal condition
            var (finalGv, finalCash) = arkMath.SimulateGvCashGrowth(gv, cash, expected);
            Assert.IsTrue(arkMath.IsBeyondCrossover(finalGv, finalCash));

            // Check that our goal is the least possible count
            if (expected > 0) {
                var (penultimateGv, penultimateCash) = arkMath.SimulateGvCashGrowth(gv, cash, expected - 1);
                Assert.IsFalse(arkMath.IsBeyondCrossover(penultimateGv, penultimateCash));
            }
        }
    }
}