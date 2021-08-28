using System;
using System.Threading.Tasks;
using uav.Command;

namespace uav
{
    class Program
    {
        static void Main(string[] args)
        /*{
            Console.WriteLine(new Arks().TryFromString("430am", out var q));
            Console.WriteLine(q);
        }

        static void Main2(string[] args)*/
        {
            var uav = new UAV();

            uav.Start(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
