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

            if (expectedMinimumCredits == expectedMaximumCredits)
            {
                return $"{gv} is the max {IpmEmoji.ipmCredits} tier, with credits of {expectedMaximumCredits} {IpmEmoji.ipmCredits}.";
            }

            var (lower,upper) = creditService.TierRange(gv);
            var totalDatapoints = await databaseService.CountInRange(lower, upper);
            var expectedCredits = await creditService.GuessCreditsForGv(gv);
            var expectedCreditsText = expectedCredits.credits >= 10 && expectedMinimumCredits != expectedMaximumCredits ?
                $" A GV of {gv} would get {(expectedCredits.accurate ? "exactly" : "approximately")} {expectedCredits.credits} {IpmEmoji.ipmCredits}." :
                string.Empty;
            return $"This tier's base {IpmEmoji.ipmCredits} range is {expectedMinimumCredits} {IpmEmoji.ipmCredits} through {expectedMaximumCredits - 1} {IpmEmoji.ipmCredits}. In this range, we have {totalDatapoints} data point(s).{expectedCreditsText}";
        }

        private static Random rng = new Random();
        private readonly static string[] thankYous = new[] {
            $"{IpmEmoji.partying_face}{IpmEmoji.partying_face}{IpmEmoji.partying_face}",
            $"{IpmEmoji.tada}",
            $"Amazeballs!",
            $"Awesome!",
            $"Bonus!",
            $"Dynamite!",
            $"Excellent contribution.",
            $"Gadzooks!",
            $"Great job!",
            $"Ho-ho!",
            $"Holy Guacamole!",
            $"Holy pretzel!",
            $"Holy Shnikies!",
            $"Hurrah!",
            $"Huzzah!",
            $"I'll be damned!",
            $"Informative!",
            $"Jeepers creepers.",
            $"Jiminy Crickets.",
            $"Jinkies!",
            $"Nice!",
            $"Olé!",
            $"Om nom nom.",
            $"Son of a gun!",
            $"Spot on.",
            $"Thank you for feeding the algorithm.",
            $"That's gonna help, for sure!",
            $"Va-va-voom!",
            $"W00t!",
            $"Well, I'll be jitterbugged.",
            $"Whoopee!",
            $"Winner winner chicken dinner!",
            $"Wow such data!",
            $"Wowie zowie!",
            $"Wowzers!",
            $"Yee-haw!",
            $"Yesssss!",
            $"Yippee!",
            $"You did it!",
            $"Zoinks!",
        };
        private static string ThankYou => thankYous[(int)(rng.NextDouble() * thankYous.Length)];

        public async Task<(string message, bool success)> UpdateCredits(string gvInput, string creditInput, IDbUser user)
        {
            if (!int.TryParse(creditInput, out var credits))
            {
                return ($@"Credit value of ""{creditInput}"" provided for GV ""{gvInput}"" isn't a number - did you put a GV value instead?", false);
            }

            return await UpdateCredits(gvInput, credits, user);
        }

        public async Task<(string message, bool success)> UpdateCredits(string gvInput, int credits, IDbUser user)
        {
            var creditService = new Credits();

            if (!GV.TryFromString(gvInput, out var gv, out var error))
            {
                return ($@"Invalid GV ""{gvInput}""{(error != null ? $"\n{error}" : string.Empty)}.", false);
            }

            if (gv < 10_000_000 || gv > 1e109)
            {
                return ($@"""{gv}"" is not between 10M and 1E+109.", false);
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
                    user_id = user.User_Id,
                };
                var (min, max) = creditService.TierRange(gv);
                var (atThisCredit, inTier) = await databaseService.AddArkValue(value, min, max, user);

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