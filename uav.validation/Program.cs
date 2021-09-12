using System;
using System.Linq;
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
                _ => NotValidParam(args[0]),
            });
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
                if (data.Length == 0)
                {
                    Console.WriteLine($"Tier: {entries.tier} -- no data available");
                    continue;
                }

                var xdata = entries.minGv.AndThen(data.Select(d => d.gv)).ToArray();
                var ydata = ((double)entries.minCredit).AndThen(data.Select(d => (double)d.baseCredits)).ToArray();

                var (b, m) = Fit.Line(xdata, ydata);
                Console.WriteLine($"Tier {entries.tier}: {data.Length} entries: credits = {m}gv + {b}");
            }
        }
    }
}
