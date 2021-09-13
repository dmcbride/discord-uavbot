using System;
using System.Threading.Tasks;
using uav.Command;

namespace uav
{
    class Program
    {
        static void Main(string[] args)
        {
            var uav = new UAV();

            uav.Start(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
