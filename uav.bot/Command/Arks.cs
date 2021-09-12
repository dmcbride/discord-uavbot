using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.logic.Constants;
using uav.logic.Database;
using uav.logic.Database.Model;
using uav.logic.Extensions;
using uav.logic.Models;
using uav.logic.Service;

namespace uav.Command
{
    public class Arks : CommandBase
    {
        private readonly Credits creditService = new Credits();
        private readonly DatabaseService databaseService = new DatabaseService();
        private readonly Ark arkService = new Ark();

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
        public async Task Ark(string gv, string goalGv, string cash = null)
        {
            cash ??= gv;

            if (!GV.TryFromString(gv, out var gvValue, out var error) ||
                !GV.TryFromString(goalGv, out var goalGvValue, out error) ||
                !GV.TryFromString(cash, out var cashValue, out error))
            {
                await ReplyAsync($"Invalid input.  Usage: `!ark currentGV goalGV cashOnHand`{(error != null ? $"\n{error}" : string.Empty)}");
                return;
            }

            if (goalGvValue < gvValue)
            {
                await ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
                return;
            }

            if (cashValue > gvValue)
            {
                await ReplyAsync($"Your cash on hand is more than your current GV, that's probably wrong.");
                return;
            }

            if (cashValue < gvValue * 0.54)
            {
                await ReplyAsync($"This calculator does not (yet) handle cash-on-hand under 54% of your current GV. You are better off not arking yet anyway. Focus on ores and getting to the end-game items, such as {Emoji.itemTP} and {Emoji.itemFR} first.");
                return;
            }

            var arks = ArkCalculate(gvValue, goalGvValue, cashValue, 1.0475d);

            // here we're assuming that you get about 7 cash arks per hour (6 minutes per ark, 10 arks per hour, 70% cash)
            var hours = Math.Floor(arks / (cashArkChance * arksPerHour));

            // and then if we got that many arks in that time, we should get about 30/70 of that in DM.
            var dm = Math.Floor(arks * dmArkChance / cashArkChance);

            await ReplyAsync(
                $@"To get to a GV of {goalGvValue} from {gvValue} starting with cash-on-hand of {cashValue}, you need {arks} {Emoji.boostcashwindfall} arks.
At about {arksPerHour * cashArkChance} {Emoji.boostcashwindfall} arks per hour, that is about {hours} hour{(hours == 1 ? string.Empty:"s")}.
During this time, you can expect to get about {dm} {Emoji.ipmdm} arks, for a total of {5 * dm} {Emoji.ipmdm}.");
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
            return ReplyAsync($"To get to a GV of {goalGvValue} from {gvValue}, you need {cws} cash windfalls. This may cost up to {dmRequired} {Emoji.ipmdm}");
        }

        [Command("basecred")]
        [Summary("Tell UAV about the base credits you get for your current GV, or query the range allowed for that GV tier.")]
        [Usage("currentGV [baseCredits [gv2 credits2] ...] ")]
        public async Task BaseCredits(params string[] parameters)
        {
            GV gvValue;
            {
                string error = null;
                if (parameters.Length == 0 || !GV.TryFromString(parameters[0], out gvValue, out error))
                {
                    await ReplyAsync($"Invalid input. Usage: `!basecred currentGV baseCredits`{(error != null ? $"\n{error}" : string.Empty)}");
                    return;
                }
            }

            if (gvValue < 10_000_000 || gvValue > 1e100)
            {
                await ReplyAsync($"Invalid input. GV must be between 10M and 1E+100");
                return;
            }

            if (parameters.Length == 1)
            {
                var msg = await arkService.QueryCreditRange(gvValue);

                await ReplyAsync(msg);
                return;
            }

            if (parameters.Length % 2 != 0)
            {
                await ReplyAsync("When entering more than one set of credits, you have to provide an even number of parameters as GV and base-credit pairs.");
                return;
            }

            var messages = parameters.NAtATime(2)
                .SelectAsync(a => arkService.UpdateCredits(a[0], a[1], Context.User.ToString()));
            var sb = new StringBuilder();
            var savedAny = false;
            await foreach (var msg in messages)
            {
                sb.AppendLine(msg.message);
                savedAny = savedAny || msg.success;
            }

            if (sb.Length > 0)
            {
                if (savedAny)
                {
                    var contributionCount = await databaseService.CountByUser(Context.User.ToString());
                    if (contributionCount.total == 1)
                    {
                        sb.AppendLine($"Thank you for your very first contribution!");
                    }
                    else
                    {
                        sb.AppendLine($"You have now contributed **{contributionCount.total}** data point(s), **{contributionCount.distinctBaseCredits}** different base credits, across **{contributionCount.distinctTiers}** tiers.");
                    }                    
                }

                await ReplyAsync(sb.ToString());
                return;
            }
        }
    }
}