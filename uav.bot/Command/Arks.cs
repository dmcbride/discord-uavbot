using System;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.Constants;
using uav.Database;
using uav.Database.Model;
using uav.logic.Models;

namespace uav.Command
{
    public class Arks : CommandBase
    {
        private readonly Credits creditService = new Credits();
        private readonly DatabaseService databaseService = new DatabaseService();

        public double ArkCalculate(double gv, double goalGv, double cash, double exponent)
        {
            var multiplier = (goalGv - (gv - cash)) / cash;

            var arks = Math.Ceiling(Math.Log(multiplier) / Math.Log(exponent));

            return arks;
        }

        private const double cashArkChance = .7d;
        private const double dmArkChance = 1 - cashArkChance;
        private const double arksPerHour = 10d;

        [Command("ark")]
        [Summary("Given your current GV, and your target GV, and cash-on-hand, how many cash arks it will take to reach your goal.")]
        [Usage("currentGV goalGV [cashOnHand]")]
        public Task Ark(string gv, string goalGv, string cash = null)
        {
            cash ??= gv;

            if (!GV.TryFromString(gv, out var gvValue, out var error) ||
                !GV.TryFromString(goalGv, out var goalGvValue, out error) ||
                !GV.TryFromString(cash, out var cashValue, out error))
            {
                return ReplyAsync($"Invalid input.  Usage: `!ark currentGV goalGV cashOnHand`{(error != null ? $"\n{error}" : string.Empty)}");
            }

            if (goalGvValue < gvValue)
            {
                return ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
            }

            if (cashValue > gvValue)
            {
                return ReplyAsync($"Your cash on hand is more than your current GV, that's probably wrong.");
            }

            if (cashValue < gvValue * 0.54)
            {
                return ReplyAsync($"This calculator does not (yet) handle cash-on-hand under 54% of your current GV. You are better off not arking yet anyway. Focus on ores and getting to the end-game items, such as {Emoji.itemTP} and {Emoji.itemFR} first.");
            }

            var arks = ArkCalculate(gvValue, goalGvValue, cashValue, 1.0475d);

            // here we're assuming that you get about 7 cash arks per hour (6 minutes per ark, 10 arks per hour, 70% cash)
            var hours = Math.Floor(arks / (cashArkChance * arksPerHour));

            // and then if we got that many arks in that time, we should get about 30/70 of that in DM.
            var dm = Math.Floor(arks * dmArkChance / cashArkChance);

            return ReplyAsync(
                $@"To get to a GV of {goalGvValue} from {gvValue} starting with cash-on-hand of {cashValue}, you need {arks} {Emoji.boostcashwindfall} arks.
At about {arksPerHour * cashArkChance} {Emoji.boostcashwindfall} arks per hour, that is about {hours} hour{(hours == 1 ? string.Empty:"s")}.
During this time, you can expect to get about {dm} {uav.Constants.Emoji.ipmdm} arks, for a total of {5 * dm} {uav.Constants.Emoji.ipmdm}.");
        }

        [Command("cw")]
        [Summary("Given your current GV, and your target GV, how many Cash Windfalls it will take to reach your goal")]
        [Usage("currentGV goalGV")]
        public Task CW(string gv, string goalGv)
        {
            if (!GV.TryFromString(gv, out var gvValue, out var error) ||
                !GV.TryFromString(goalGv, out var goalGvValue, out error))
            {
                return ReplyAsync($"Invalid input. Usage: `!cw currentGV goalGV`{(error != null ? $"\n{error}" : string.Empty)}");
            }

            if (goalGvValue < gvValue)
            {
                return ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
            }

            var cws = ArkCalculate(gvValue, goalGvValue, gvValue, 1.1);
            var dmRequired = cws * 30;
            return ReplyAsync($"To get to a GV of {goalGvValue} from {gvValue}, you need {cws} cash windfalls. This may cost up to {dmRequired} {uav.Constants.Emoji.ipmdm}");
        }

        [Command("basecred")]
        [Summary("Tell UAV about the base credits you get for your current GV, or query the range allowed for that GV tier.")]
        [Usage("currentGV [baseCredits]")]
        public async Task BaseCredits(string gv, int? credits = null)
        {
            if (!GV.TryFromString(gv, out var gvValue, out var error))
            {
                await ReplyAsync($"Invalid input. Usage: `!basecred currentGV baseCredits`{(error != null ? $"\n{error}" : string.Empty)}");
                return;
            }

            if (gvValue < 10_000_000 || gvValue > 1e100)
            {
                await ReplyAsync($"Invalid input. GV must be between 10M and 1E+100");
                return;
            }

            var expectedMinimumCredits = creditService.TierCredits(gvValue);
            var expectedMaximumCredits = creditService.TierCredits(gvValue * 10);
            if (credits == null)
            {
                var (lower,upper) = creditService.TierRange(gvValue);
                var totalDatapoints = await databaseService.CountInRange(lower, upper);
                var msg = $"This tier's base credits range is {expectedMinimumCredits} through {expectedMaximumCredits - 1}. In this range, we have {totalDatapoints} data point(s).";
                if (expectedMinimumCredits == expectedMaximumCredits)
                {
                    msg = $"This is the max tier, with credits of {expectedMaximumCredits}";
                }
                await ReplyAsync(msg);
                return;
            }

            if (credits < expectedMinimumCredits || expectedMaximumCredits < credits)
            {
                await ReplyAsync($"The given credits of {credits} lies outside the expected range for this tier of {expectedMinimumCredits} - {expectedMaximumCredits}. If this is incorrect, please send a screen-cap to Tanktalus showing this.");
                return;
            }

            var channel = await Context.User.GetOrCreateDMChannelAsync();
            try
            {
                var value = new ArkValue
                {
                    BaseCredits = credits.Value,
                    Gv = gvValue,
                    Reporter = Context.User.ToString()
                };
                var (min, max) = creditService.TierRange(gvValue);
                var (atThisCredit, inTier) = await databaseService.AddArkValue(value, min, max);                

                // send back private reply
                await channel.SendMessageAsync($"Thank you for feeding the algorithm.  Recorded that your current GV of {gvValue} gives base credits of {credits}. There are now {inTier} report(s) in this tier and {atThisCredit} report(s) for this base credit value.");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}