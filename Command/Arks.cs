using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.Extensions;

namespace uav.Command
{
    public class Arks : ModuleBase<SocketCommandContext>
    {

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

            if (!TryFromString(gv, out var gvValue) ||
                !TryFromString(goalGv, out var goalGvValue) ||
                !TryFromString(cash, out var cashValue))
            {
                return ReplyAsync($"Invalid input.  Usage: `!ark currentGV goalGV cashOnHand`");
            }

            if (goalGvValue < gvValue)
            {
                return ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
            }

            if (cashValue > gvValue)
            {
                return ReplyAsync($"Your cash on hand is more than your current GV, that's probably wrong.");
            }

            var arks = ArkCalculate(gvValue, goalGvValue, cashValue, 1.0475d);

            // here we're assuming that you get about 6 cash arks per hour (6 minutes per ark, 10 arks per hour, 60% cash)
            var hours = Math.Floor(arks / 6);

            // and then if we got that many arks in that time, we should get about 40/60 of that in DM.
            var dm = Math.Floor(arks * 40 / 60);

            return ReplyAsync(
                $@"To get to a GV of {DualString(goalGvValue)} from {DualString(gvValue)} starting with cash-on-hand of {DualString(cashValue)}, you need {arks} <:boostcashwindfall:642974216681029644> arks.
At about 6 <:boostcashwindfall:642974216681029644> arks per hour, that is about {hours} hour{(hours == 1 ? string.Empty:"s")}.
During this time, you can expect to get about {dm} <:ipmdm:628302645265956875> arks, for a total of {5 * dm} <:ipmdm:628302645265956875>.");
        }

        [Command("cw")]
        [Summary("Given your current GV, and your target GV, how many Cash Windfalls it will take to reach your goal")]
        [Usage("currentGV goalGV")]
        public Task CW(string gv, string goalGv)
        {
            if (!TryFromString(gv, out var gvValue) ||
                !TryFromString(goalGv, out var goalGvValue))
            {
                return ReplyAsync($"Invalid input. Usage: `!cw currentGV goalGV");
            }

            if (goalGvValue < gvValue)
            {
                return ReplyAsync($"Your goal is already reached. Perhaps you meant to reverse them?");
            }

            var cws = ArkCalculate(gvValue, goalGvValue, gvValue, 1.1);
            return ReplyAsync($"To get to a GV of {DualString(goalGvValue)} from {DualString(gvValue)}, you need {cws} cash windfalls.");
        }

        private static readonly IReadOnlyDictionary<string, int> suffixExponent;
        private static readonly IReadOnlyDictionary<int, string> toSuffixExponent;

        static Arks()
        {
            var s = new[] {
                "k", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "D"
                }.Concat(Enumerable.Range((int)'a', (int)'m' - (int)'a' + 1).Select(i => "a" + (char)i))
                .Select((s, i) => (s, i: 3*(i+1)))
                .ToDictionary(k => k.s, k => k.i);
            toSuffixExponent = s.ToDictionary(kv => kv.Value, kv => kv.Key); // reverse lookup
            s["m"] = s["M"];
            s["K"] = s["k"];
            suffixExponent = s;
        }

        private static readonly Regex SiNumber = new Regex(@"^(?<qty>\d+(?:\.\d+)?|\.\d+)(?<suffix>[a-zA-Z]{0,2})$");
        private static readonly Regex ExpNumber = new Regex(@"^(?<qty>\d+(?:\.\d+)?|\.\d+)[eE](?<exp>\d+)$");

        public bool TryFromString(string v, out double qty)
        {
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