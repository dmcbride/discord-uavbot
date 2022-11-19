using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Models;

namespace uav.test
{
    [TestClass]
    public class GvCashTest
    {
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
            // Check our expected counts
            var gvCash = GvCashPair.FromNumbers(gv, cash);
            var actual = gvCash.CountArksUntilCrossover();
            Assert.AreEqual(expected, actual);

            // Check our goal condition
            var final = gvCash.SimulateGrowth(expected);
            Assert.IsTrue(final.IsBeyondCrossover());

            // Check that our goal is the least possible count
            if (expected > 0)
            {
                var penultimate = gvCash.SimulateGrowth(expected - 1);
                Assert.IsFalse(penultimate.IsBeyondCrossover());
            }
        }
    }
}