using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.Extensions;
using uav.Constants;
using uav.Database;
using uav.Database.Model;
using uav.Models;

namespace uav.Command
{
    public class Arks : ModuleBase<SocketCommandContext>
    {
        private readonly Credits creditService = new Credits();
        private readonly DatabaseService databaseService = new DatabaseService();

        public double ArkCalculate(double gv, double goalGv, double cash, double exponent)
        {
            var multiplier = (goalGv - (gv - cash)) / cash;

            var arks = Math.Ceiling(Math.Log(multiplier) / Math.Log(exponent));

            return arks;
        }

        [Command("ark")]
        [Summary("Given your current GV, and your target GV, and cash-on-hand, how many cash arks it will take to reach your goal.")]
        [Usage("currentGV goalGV [cashOnHand]")]
        public Task Ark(string gv, string goalGv, string cash = null)
        {
            cash ??= gv;

            if (!TryFromString(gv, out var gvValue, out var error) ||
                !TryFromString(goalGv, out var goalGvValue, out error) ||
                !TryFromString(cash, out var cashValue, out error))
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

            // here we're assuming that you get about 6 cash arks per hour (6 minutes per ark, 10 arks per hour, 60% cash)
            var hours = Math.Floor(arks / 6);

            // and then if we got that many arks in that time, we should get about 30/70 of that in DM.
            var dm = Math.Floor(arks * 30 / 70);

            return ReplyAsync(
                $@"To get to a GV of {DualString(goalGvValue)} from {DualString(gvValue)} starting with cash-on-hand of {DualString(cashValue)}, you need {arks} {Emoji.boostcashwindfall} arks.
At about 6 {Emoji.boostcashwindfall} arks per hour, that is about {hours} hour{(hours == 1 ? string.Empty:"s")}.
During this time, you can expect to get about {dm} {uav.Constants.Emoji.ipmdm} arks, for a total of {5 * dm} {uav.Constants.Emoji.ipmdm}.");
        }

        [Command("cw")]
        [Summary("Given your current GV, and your target GV, how many Cash Windfalls it will take to reach your goal")]
        [Usage("currentGV goalGV")]
        public Task CW(string gv, string goalGv)
        {
            if (!TryFromString(gv, out var gvValue, out var error) ||
                !TryFromString(goalGv, out var goalGvValue, out error))
            {
                return ReplyAsync($"Invalid input. Usage: `!cw currentGV goalGV`{(error != null ? $"\n{error}" : string.Empty)}");
            }

            if (goalGvValue < gvValue)
            {
                return ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
            }

            var cws = ArkCalculate(gvValue, goalGvValue, gvValue, 1.1);
            var dmRequired = cws * 30;
            return ReplyAsync($"To get to a GV of {DualString(goalGvValue)} from {DualString(gvValue)}, you need {cws} cash windfalls. This may cost up to {dmRequired} {uav.Constants.Emoji.ipmdm}");
        }

        [Command("basecred")]
        [Summary("Tell UAV about the base credits you get for your current GV, or query the range allowed for that GV tier.")]
        [Usage("currentGV [baseCredits]")]
        public async Task BaseCredits(string gv, int? credits = null)
        {
            if (!TryFromString(gv, out var gvValue, out var error))
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
                await channel.SendMessageAsync($"Thank you for feeding the algorithm.  Recorded that your current GV of {DualString(gvValue)} gives base credits of {credits}. There are now {inTier} report(s) in this tier and {atThisCredit} report(s) for this base credit value.");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static readonly IReadOnlyDictionary<string, int> suffixExponent;
        private static readonly IReadOnlyDictionary<int, string> toSuffixExponent;
        private static readonly IReadOnlyList<string> recommendedSuffixes;

        static Arks()
        {
            recommendedSuffixes = new[] {
                "k", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "D"
                }.Concat(Enumerable.Range((int)'a', (int)'m' - (int)'a' + 1).Select(i => "a" + (char)i))
                .ToList();
            var s = recommendedSuffixes
                .Select((s, i) => (s, i: 3*(i+1)))
                .ToDictionary(k => k.s, k => k.i);
            toSuffixExponent = s.ToDictionary(kv => kv.Value, kv => kv.Key); // reverse lookup
            s["m"] = s["M"];
            s["K"] = s["k"];
            suffixExponent = s;
        }

        private static readonly Regex SiNumber = new Regex(@"^(?<qty>\d+(?:\.\d+)?|\.\d+)(?<suffix>[a-zA-Z]{0,2})$");
        private static readonly Regex ExpNumber = new Regex(@"^(?<qty>\d+(?:\.\d+)?|\.\d+)[eE]\+?(?<exp>\d+)$");

        public bool TryFromString(string v, out double qty, out string errorMessage)
        {
            errorMessage = null;
            var m = SiNumber.Match(v);
            qty = 0d;
            if (m.Success)
            {
                qty = Double.Parse(m.Groups["qty"].Value);
                var suffix = m.Groups["suffix"].Value;
                if (suffixExponent.TryGetValue(suffix, out var exp))
                {
                    qty *= Math.Pow(10d, exp);
                }
                else if (!suffix.IsNullOrEmpty()) // you tried something else. Don't do that.
                {
                    if (suffixExponent.TryGetValue(suffix.ToLowerInvariant(), out _) ||
                        suffixExponent.TryGetValue(suffix.ToUpperInvariant(), out _))
                    {
                        errorMessage = $"Perhaps you got the case wrong. Suffixes are allowed to be one of: {string.Join(",", recommendedSuffixes)}.";
                    }
                    return false;
                }
                return true;
            }
            else if ((m = ExpNumber.Match(v)).Success)
            {
                qty = Double.Parse(m.Groups["qty"].Value);
                qty *= Math.Pow(10d, Double.Parse(m.Groups["exp"].Value));
                return true;
            }
            return false;
        }

        private (string letter, string exp) ToStrings(double qty)
        {
            var powerOf10 = (int) Math.Floor(Math.Log10(qty));
            var powerOf1000 = powerOf10 / 3;

            string letter = null;
            if (toSuffixExponent.TryGetValue(powerOf1000 * 3, out var letterSuffix))
            {
                letter = $"{qty / Math.Pow(1000d, powerOf1000):g5}{letterSuffix}";
            }

            var exp = $"{qty / Math.Pow(10d, powerOf10):g2}E+{powerOf10}";

            return (letter, exp);
        }

        private string DualString(double qty)
        {
            var (letter, exp) = ToStrings(qty);

            return letter == null ? exp : $"{letter} ({exp})";
        }
    }
}