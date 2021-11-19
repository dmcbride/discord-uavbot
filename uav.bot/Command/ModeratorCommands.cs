using System;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.logic.Models;

namespace uav.Command
{
    public class ModeratorCommands : CommandBase
    {
        [Command("tier")]
        [Summary("Tells you which tier this is")]
        [Usage("tier")]
        public async Task Tier(int tierNumber)
        {
            var gv = GV.FromNumber(Math.Pow(10d, tierNumber + 6));
            await ReplyAsync($"Tier {tierNumber} starts at {gv}.");
        }

    }
}