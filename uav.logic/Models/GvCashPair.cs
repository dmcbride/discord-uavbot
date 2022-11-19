using System;

namespace uav.logic.Models
{
    // A pair that represents a galaxy's total value and also the cash on hand
    public class GvCashPair
    {
        public const double cashArkFactor = 0.0475d;
        public const double gvArkFactor = 0.02295d;
        public const double cashWindfallFactor = 0.1d;

        public readonly GV gv;
        public readonly GV cash;

        public GvCashPair(GV gv, GV cash)
        {
            this.gv = gv;
            this.cash = cash;
        }

        public static GvCashPair FromNumbers(double gv, double cash)
        {
            return new GvCashPair(GV.FromNumber(gv), GV.FromNumber(cash));
        }

        public int CountArksUntilCrossover()
        {
            return CountUntilCrossover(gvArkFactor);
        }

        public bool IsBeyondCrossover()
        {
            return gv * gvArkFactor <= cash * cashArkFactor;
        }

        public GvCashPair SimulateGrowth(int count, double gvIncomeFactor = gvArkFactor)
        {
            var newGv = gv * Math.Pow(1 + gvIncomeFactor, count);
            var newCash = cash + (newGv - gv);
            return new GvCashPair(GV.FromNumber(newGv), GV.FromNumber(newCash));
        }

        // The goal is to find the smallest non-negative count of income iterations, until the following is satisfied:
        //
        //      newGv(count) * gvArkFactor <= newCash(count) * cashArkFactor
        //
        // where newGv(...) and newCash(...) are determined as:
        //
        //      newGv(count) := gv * Math.pow(1 + gvIncomeFactor, count)
        //      newCash(count) := cash + (newGv(count) - gv)
        //
        // Assumes 0 <= cash <= gv
        // Assumes 0 < gvIncomeFactor
        // Assumes 0 < gvArkFactor < cashArkFactor
        //
        // For practical purposes, gvIncomeFactor can either be gvArkFactor or cashWindfallFactor
        private int CountUntilCrossover(double gvIncomeFactor)
        {
            // Check if the crossover already has happened without any new income
            if (IsBeyondCrossover())
            {
                return 0;
            }

            // We can mathematically derive an efficient log-based formula.
            //
            // We can expand newCash(count) in terms of newGv(count) and also mathematically rearrange the crossover inequality:
            //
            //      newGv(count) * gvArkFactor <= newCash(count) * cashArkFactor
            //      newGv(count) * gvArkFactor / cashArkFactor <= newCash(count)
            //      newGv(count) * gvArkFactor / cashArkFactor <= cash + (newGv(count) - gv)
            //      -cash + gv <= newGv(count) - newGv(count) * gvArkFactor / cashArkFactor
            //      gv - cash <= newGv(count) * (1 - gvArkFactor / cashArkFactor)
            //      (gv - cash) / (1 - gvArkFactor / cashArkFactor) <= newGv(count)
            //
            // Next, we can expand newGv(count) in terms of the count we seek and also further rearrange:
            //
            //      (gv - cash) / (1 - gvArkFactor / cashArkFactor) <= gv * Math.pow(1 + gvIncomeFactor, count)
            //      (1 - cash / gv) / (1 - gvArkFactor / cashArkFactor) <= Math.pow(1 + gvIncomeFactor, count)
            //
            // At last, we can take logarithms to isolate the count on one side.
            //
            //      Math.Log((1 - cash / gv) / (1 - gvArkFactor / cashArkFactor), 1 + gvIncomeFactor) <= count
            //
            // The smallest integer is now solvable with a Math.Ceiling on the left hand side.

            var cashGapAsRatio = 1 - cash / gv;
            var factorGapAsRatio = 1 - gvArkFactor / cashArkFactor;

            var goalIncomeMultiplier = cashGapAsRatio / factorGapAsRatio;
            var countExponent = Math.Log(goalIncomeMultiplier, 1 + gvIncomeFactor);

            return Convert.ToInt32(Math.Ceiling(countExponent));
        }
    }
}