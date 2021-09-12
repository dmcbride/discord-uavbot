using System.Threading.Tasks;
using uav.logic.Database;
using uav.logic.Models;
using uav.logic.Constants;
using System;
using uav.logic.Database.Model;

namespace uav.logic.Service
{
    public class Ark
    {
        private DatabaseService databaseService = new DatabaseService();
        
        public async Task<string> QueryCreditRange(GV gv)
        {
            var creditService = new Credits();
            var databaseService = new DatabaseService();

            var expectedMinimumCredits = creditService.TierCredits(gv);
            var expectedMaximumCredits = creditService.TierCredits(gv, 1);

            var (lower,upper) = creditService.TierRange(gv);
            var totalDatapoints = await databaseService.CountInRange(lower, upper);
            var msg = $"This tier's base {Emoji.ipmCredits} range is {expectedMinimumCredits} {Emoji.ipmCredits} through {expectedMaximumCredits - 1} {Emoji.ipmCredits}. In this range, we have {totalDatapoints} data point(s).";
            if (expectedMinimumCredits == expectedMaximumCredits)
            {
                msg = $"This is the max {Emoji.ipmCredits} tier, with credits of {expectedMaximumCredits} {Emoji.ipmCredits}";
            }

            return msg;
        }

        private static Random rng = new Random();
        private readonly static string[] thankYous = new[] {
            "Thank you for feeding the algorithm.",
            "Om nom nom.",
            "Yesssss!",
            "Awesome!",
            "That's gonna help, for sure!",
            "Nice!",
            "Bonus!",
            "Great job!",
            "Informative!",
            "Wowzers!",
            $"{Emoji.partying_face}{Emoji.partying_face}{Emoji.partying_face}",
            $"{Emoji.tada}",
            $"Holy pretzel!",
            "Well, I'll be jitterbugged.",
            "Jiminy Crickets.",
            "Gadzooks!",
            "Wowie zowie!",
            "Olé!",
            "Ho-ho!",
            "Hurrah!",
            "Va-va-voom!",
            "Whoopee!",
            "W00t!",
            "Yee-haw!",
            "Yippee!",
            "Winner winner chicken dinner!",
        };
        private static string ThankYou => thankYous[(int)(rng.NextDouble() * thankYous.Length)];

        public async Task<(string message, bool success)> UpdateCredits(string gvInput, string creditInput, string user)
        {
            var creditService = new Credits();

            if (!GV.TryFromString(gvInput, out var gv, out var error))
            {
                return ($@"Invalid GV ""{gvInput}""{(error != null ? $"\n{error}" : string.Empty)}.", false);
            }

            if (gv < 10_000_000 || gv > 1e100)
            {
                return ($@"""{gv}"" is not between 10M and 1E+100.", false);
            }

            if (!int.TryParse(creditInput, out var credits))
            {
                return ($@"Credit value of ""{creditInput}"" provided for GV ""{gv}"" isn't a number - did you put a GV value instead?", false);
            }

            var expectedMinimumCredits = creditService.TierCredits(gv);
            var expectedMaximumCredits = creditService.TierCredits(gv, 1);
            if (credits < expectedMinimumCredits || expectedMaximumCredits < credits)
            {
                return (@$"The given credits of {credits} for a GV of ""{gv}"" lies outside the expected range for tier {gv.TierNumber} of {expectedMinimumCredits} - {expectedMaximumCredits}. If this is incorrect, please send a screen-cap to Tanktalus showing this.", false);
            }

            if (credits == expectedMinimumCredits)
            {
                return ($"Thank you for the input.  However, {credits} is the minimum credits for tier {gv.TierNumber}, and is already known with precision.", false);
            }

            try
            {
                var value = new ArkValue
                {
                    Base_Credits = credits,
                    Gv = gv,
                    Reporter = user.ToString()
                };
                var (min, max) = creditService.TierRange(gv);
                var (atThisCredit, inTier) = await databaseService.AddArkValue(value, min, max);

                return ($"{ThankYou} Recorded that your current GV of **{gv}** gives base credits of **{credits}**. There are now **{inTier}** report(s) in tier {gv.TierNumber} and **{atThisCredit}** report(s) for this base credit value.", true);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
            return ("Unknown error.", false);
        }
    }
}