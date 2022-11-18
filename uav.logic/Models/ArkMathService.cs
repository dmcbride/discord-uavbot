using System;

namespace uav.logic.Models
{
    public class ArkMathService
    {
        public const double cashArkFactor = 0.0475d;
        public const double gvArkFactor = 0.02295d;
        public const double cashWindfallFactor = 0.1d;

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
        public int CountUntilCrossover(double gv, double cash, double gvIncomeFactor = gvArkFactor) {
            // Check if the crossover already has happened without any new income
            if (IsBeyondCrossover(gv, cash)) {
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

        public bool IsBeyondCrossover(double gv, double cash) {
            return gv * gvArkFactor <= cash * cashArkFactor;
        }

        public (GV, GV) SimulateGvCashGrowth(double gv, double cash, int count, double gvIncomeFactor = gvArkFactor) {
            var newGv = gv * Math.Pow(1 + gvIncomeFactor, count);
            var newCash = cash + (newGv - gv);
            return (GV.FromNumber(newGv), GV.FromNumber(newCash));
        }
    }
}