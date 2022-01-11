using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using uav.Attributes;

namespace uav.Command
{
    public class Tournament : CommandBase
    {
        [Command("nexttourn")]
        [Summary("Tells you when the next tournament will start (Removed)")]
        [Usage("nexttourn")]
        public async Task NextTourn()
        {
            var embed = EmbedBuilder("Next Tournament", "**Error**: The `!nexttourn` command has been replaced with `/next-tournament`. Please use that command instead.", Color.DarkRed);

            await ReplyAsync(embed);
        }
    }
}