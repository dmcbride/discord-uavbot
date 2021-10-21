using System;
using System.Threading.Tasks;
using uav.Command;

namespace uav
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var uav = new UAV();

            await uav.Start(args).ConfigureAwait(false);
        }
    }
}
