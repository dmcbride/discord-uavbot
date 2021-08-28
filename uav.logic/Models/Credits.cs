using System;
using System.Linq;

namespace uav.logic.Models
{
    public class Credits
    {
        private const int MaxTier = 100 - 7; // from 1E7 to 1E100
        private static int[] prestigeTierValueBase = new int[MaxTier + 1];

        static Credits()
        {
            prestigeTierValueBase[0] = 10;
            foreach (var tier in Enumerable.Range(1, MaxTier))
            {
                prestigeTierValueBase[tier] = (int)Math.Round(Math.Floor(prestigeTierValueBase[tier-1] + ((tier + 1) * 10.3f)), MidpointRounding.ToEven);
            }
        }

        public (double gvFloor, double gvCeiling) TierRange(double gv)
        {
            var tier = Math.Floor(Math.Log10(gv));
            var upperTier = Math.Min(tier + 1, 100);

            return (Math.Pow(10, tier), Math.Pow(10, upperTier));
        }

        public int TierCredits(GV gv, int offset = 0)
        {
            var tier = gv.CreditTier() + offset;
            if (tier > MaxTier)
            {
                tier = MaxTier;
            }
            return prestigeTierValueBase[tier];
        }
        // public int CreditsFor(double gv)
        // {
        //     var tier = TierFor(gv);
        //     var prestigePointsDifference = prestigeTierValueBase[tier + 1] - prestigeTierValueBase[tier];
        //     var partialRatio = (gv - )
        // }
        // prestigePointsDifference = prestigeTierValueBase[prestigeTier + 1] - prestigeTierValueBase[prestigeTier];
        //         partialRatio = (homebase.galaxyTotalValue - valueMin[prestigeTier]) / valueMax[prestigeTier];
    }
}