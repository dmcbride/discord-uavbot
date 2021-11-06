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
        private const int MaxTier = 104 - TierOffset; // from 1E7 to 1E104
        private static float[] prestigeTierValueBase = new float[MaxTier + 1];
        private DatabaseService database = new DatabaseService();

        static Credits()
        {
            prestigeTierValueBase[0] = 10;
            foreach (var tier in Enumerable.Range(1, MaxTier))
            {
                prestigeTierValueBase[tier] = (int)Math.Round(Math.Floor(prestigeTierValueBase[tier-1] + ((tier + 1) * 10.3f)), MidpointRounding.ToEven);
            }
            /*prestigeTierValueBase[95] *= .996f;
            prestigeTierValueBase[96] *= .992f;
            prestigeTierValueBase[97] *= .988f;
            prestigeTierValueBase[98] *= .984f;
            prestigeTierValueBase[99] *= .981f;
            prestigeTierValueBase[100] *= .978f;
            prestigeTierValueBase[101] *= .976f;
            prestigeTierValueBase[102] *= .974f;
            prestigeTierValueBase[103] *= .972f;
            prestigeTierValueBase[104] *= .970f;*/
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
