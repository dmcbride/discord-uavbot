using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathNet.Numerics;
using uav.logic.Database;
using uav.logic.Extensions;

namespace uav.validation
{
    class Program
    {
        static void Main(string[] args)
        {

            new Program().Start(args).ConfigureAwait(false).GetAwaiter().GetResult();

        }

        async Task Start(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No params given");
                return;
            }

            await (args[0] switch {
                "oor" => ValidateOutOfRange(),
                "slopes" => CalculateSlopes(),
                "tc" => TrialCredits(),
                "rx" => RegexCheck(),
                _ => NotValidParam(args[0]),
            });
        }

        Task RegexCheck()
        {
            (string Name, string Pet)[] Pets = {
                ("None", ""),
                ("Rabbit (credit farmer)", "🐇"),
                ("Turtle (long hauler)", "🐢"),
                ("Dragon (tourney/challenge)", "🐲"),
                ("Unicorn (tourney/challenge)", "🦄"),
                ("Worm (tourney only)", "🪱"),
                ("Whale ($$$)", "🐳"),
                ("Robot (UAV Mod)", "🤖"),
                ("Bat (Man)", "🦇"),
                ("Penguin (Arms)", "🐧"),
            };

            Regex PetFinder = new Regex($"^((?:{string.Join("|",Pets.Where(p=>p.Pet.Any()).Select(p=>p.Pet))}) ?)?");

            var realNick = "🪱 D★nger *³²²^²¹¹ 💫";
            var realChangedNick = PetFinder.Replace(realNick, Pets[1].Pet);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"Renaming {realNick} to {realChangedNick}:\n[{string.Join(",", realNick.Select(c => $"{c}:{(int)c:x}"))}]\n[{string.Join(",", realChangedNick.Select(c => $"{c}:{(int)c:x}"))}]");

            string superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            char turtlePrefix = '^';
            char rabbitPrefix = '*';

            var rx = $" ?[{superscripts}{turtlePrefix}{rabbitPrefix}]+(?=\\s*[^{superscripts}{turtlePrefix}{rabbitPrefix}]*?$)";
            var findSuper = Regex.Replace(realNick, rx, "---");
            Console.WriteLine($"No superscripts: {findSuper}");

            return Task.CompletedTask;
        }

        Task NotValidParam(string p)
        {
            Console.WriteLine($"Unknown parameter: {p}");
            return Task.CompletedTask;
        }

        async Task ValidateOutOfRange()
        {
            var db = new DatabaseService();
            var badEntries = await db.FindOutOfRange();

            foreach (var entry in badEntries)
            {
                Console.WriteLine($"{entry.Reporter} : {entry.Gv:g5} : {entry.Base_Credits}");
            }
        }

        async Task CalculateSlopes()
        {
            var db = new DatabaseService();

            await foreach (var entries in db.DataByTier())
            {
                var data = entries.data.ToArray();
                if (data.Length <= 1)
                {
                    Console.WriteLine($"Tier: {entries.tier} -- no data available");
                    continue;
                }

                //var xdata = entries.minGv.AndThen(data.Select(d => d.gv)).ToArray();
                //var ydata = ((double)entries.minCredit).AndThen(data.Select(d => (double)d.baseCredits)).ToArray();
                var xdata = data.Select(d => d.gv).ToArray();
                var ydata = data.Select(d => (double)d.baseCredits).ToArray();

                var (b, m) = Fit.Line(xdata, ydata);
                if (b > entries.minCredit)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine($"Tier {entries.tier} ({entries.minGv} = {entries.minCredit}): {data.Length} entries: credits = {m}gv + {b}");
                Console.ResetColor();
            }
        }

        Task TrialCredits()
        {
            var trials = new double[] {
                1e7,
                5e7,
                9e7,
                9.99e7,
                1e8,
                1.1e8,
                1.2e8,
                5e8,
                9.99e8,
                1e9,
                1e99,
                1.1e99,
                1.2e99,
                5e99,
                9e99,
                9.99e99,
                1e100,
                1e101,
            };
            foreach (var gv in trials)
            {
                Console.WriteLine($"{gv:g} gives {CalcCredit(gv)}");
            }
            return Task.CompletedTask;
            
            int CalcCredit(double gv)
            {
                const double maxGv = 1e100;

                if (gv > maxGv)
                {
                    gv = maxGv;
                }

                var tier = (int) Math.Floor(Math.Log10(gv)) - 6;
                var tierBaseGv = Math.Pow(10, tier + 6);
                var nextTierBaseGv = Math.Pow(10, tier + 7);

                var baseCreditPerTier = 10.3d;
                var baseCredit = (int) (baseCreditPerTier * tier * (tier + 1) / 2);
                var nextTierBaseCredit = (int) (baseCreditPerTier * (tier + 1) * (tier + 2) / 2);

                var credits = baseCredit +
                    (int)((nextTierBaseCredit - baseCredit) * .9 * (gv - tierBaseGv) / (nextTierBaseGv - tierBaseGv));

                return credits;
            }
        }
    }
}
