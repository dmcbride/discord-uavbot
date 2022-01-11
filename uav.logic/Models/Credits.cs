using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics;
using uav.logic.Database;

namespace uav.logic.Models
{
    public class Credits
    {
        private const int TierOffset = 6; // 10m
        private const int MaxTier = 109 - TierOffset; // from 1E7 to 1E109
        private static float[] prestigeTierValueBase = new float[MaxTier];
        private DatabaseService database = new DatabaseService();

        static Credits()
        {
            prestigeTierValueBase[0] = 10;
            foreach (var tier in Enumerable.Range(1, MaxTier - 1))
            {
                prestigeTierValueBase[tier] = (int)Math.Round(Math.Floor(prestigeTierValueBase[tier-1] + ((tier + 1) * 10.3f)), MidpointRounding.ToEven);
            }
            prestigeTierValueBase[93] = 46666; // *= .996f; // 46666
            prestigeTierValueBase[94] = 47458; // *= .992f; // 47458
            prestigeTierValueBase[95] = 48253; // *= .988f; // 48253
            prestigeTierValueBase[96] = 49046; // *= .984f; // 49046
            prestigeTierValueBase[97] = 49895; // *= .981f; // 49895
            prestigeTierValueBase[98] = 50007; // *= .978f; // 50007
            prestigeTierValueBase[99] = 50913; // *= .976f; // 50913
            prestigeTierValueBase[100] = 51824; // *= .974f; // 51824
            prestigeTierValueBase[101] = 52741; // *= .972f; // 52741
            prestigeTierValueBase[102] = 54505; // *= .970f; // 54505
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
            if (tier >= MaxTier)
            {
                tier = MaxTier - 1;
            }
            return (int)prestigeTierValueBase[tier];
        }

        public IEnumerable<(int tier, double gvMin, double gvMax, int creditMin, int creditMax)> AllTiers()
        {
            foreach (var tier in Enumerable.Range(1, MaxTier - 1))
            {
                var nextTier = Math.Min(tier + 1, MaxTier);
                var gvMin = Math.Pow(10, tier + TierOffset);
                var gvMax = Math.Pow(10, nextTier + TierOffset);
                yield return (tier, gvMin, gvMax, (int)prestigeTierValueBase[tier], (int)prestigeTierValueBase[nextTier]);
            }
        }

        public async Task<(int credits, bool accurate)> GuessCreditsForGv(GV gv)
        {
            var data = (await database.GetMinMaxValuesForCredits(gv))
                .OrderBy(d => d.gv)
                .ToArray();
            var (minGv, _) = gv.TierRange();

            // TODO: need to improve this performance.
            if (
                data.Any() && (
                    data.LastOrDefault(d => d.gv <= gv)?.base_credits == data.FirstOrDefault(d => d.gv >= gv)?.base_credits ||
                    data.Any(d => d.gv == gv)
                )
            )
            {
                return (data.FirstOrDefault(d => d.gv >= gv)?.base_credits ?? data.Last(d => d.gv < gv).base_credits, true);
            }

            if (data.Length < 10)
            {
                return (-1, false);
            }

            var xdata = data.Select(d => d.gv).ToArray();
            var ydata = data.Select(d => (double)d.base_credits).ToArray();

            var (b, m) = Fit.Line(xdata, ydata);
            if (b > minGv)
            {
                return (-1, false);
            }            

            return ((int) Math.Floor(m * gv + b), false);
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
