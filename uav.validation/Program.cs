using System;
using System.Threading.Tasks;
using uav.logic.Database;

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
                Console.WriteLine($"{entry.Reporter} : {entry.Gv:g5} : {entry.BaseCredits}");
            }
        }

    }
}
