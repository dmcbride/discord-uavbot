using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using uav.Attributes;
using uav.logic.Constants;
using uav.logic.Extensions;
using IpmEmoji = uav.logic.Constants.IpmEmoji;

namespace uav.Command
{
    public class Tournament : CommandBase
    {
        [Command("nexttourn")]
        [Summary("Tells you when the next tournament will start")]
        [Usage("nexttourn")]
        public async Task NextTourn()
        {
            var now = DateTime.UtcNow;

            var embed = EmbedBuilder("Next Tournament", NextTournReply(now), Color.DarkRed);

            await ReplyAsync(embed);
        }

        private record NextTournMessage (DayOfWeek ByEndOf, string Message, DayOfWeek? next = null);

        private NextTournMessage[] nextTournMessages = {
            new (DayOfWeek.Thursday, $"The next tournamanet starts in {{0}}. Don't forget to sign up to a guild! Good luck! {IpmEmoji.four_leaf_clover}", DayOfWeek.Friday),
            new (DayOfWeek.Friday, $"The next tournament starts in {{0}}. Oh the anticipation is killing me! Good luck! {IpmEmoji.four_leaf_clover}"),
            new (DayOfWeek.Saturday, $"The tournament is already going on!  You only have {{0}} left to join it! Good luck! {IpmEmoji.four_leaf_clover}"),
            new (DayOfWeek.Sunday, $"The tournament is going on, but you can no longer join it. The next tournament will start in {{0}}.", DayOfWeek.Friday),
        };

        public string NextTournReply(DateTime now)
        {
            var msg = nextTournMessages.FirstOrDefault(m => m.ByEndOf >= now.DayOfWeek) ?? nextTournMessages.First();

            var daysUntil = (msg.next ?? msg.ByEndOf) - now.DayOfWeek;
            if (daysUntil < 0)
            {
                daysUntil += 7;
            }
            var nextTime = now.Date.AddDays(daysUntil + 1);

            return @$"Warning: The `!nexttourn` command is being replaced with `/next-tournament` and will go away. Please use `/next-tourn` in the future. {IpmEmoji.warning}\n\n" +
                string.Format(msg.Message, SpanToReadable(nextTime - now)) + "\n\n" + Support.SupportStatement;
        }

        private string SpanToReadable(TimeSpan span) => uav.logic.Models.Tournament.SpanToReadable(span);
    }
}