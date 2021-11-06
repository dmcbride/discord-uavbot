using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using uav.Attributes;
using uav.logic.Constants;

namespace uav.Command
{
    public class Tournament : ModuleBase<SocketCommandContext>
    {
        [Command("nexttourn")]
        [Summary("Tells you when the next tournament will start")]
        [Usage("nexttourn")]
        public async Task NextTourn()
        {
            var now = DateTime.UtcNow;

            await ReplyAsync(NextTournReply(now));
        }

        private record NextTournMessage (DayOfWeek ByEndOf, string Message, DayOfWeek? next = null);

        private NextTournMessage[] nextTournMessages = {
            new (DayOfWeek.Thursday, $"The next tournamanet starts in {{0}}. Don't forget to sign up to a `!guild`! Good luck! {Emoji.four_leaf_clover}", DayOfWeek.Friday),
            new (DayOfWeek.Friday, $"The next tournament starts in {{0}}. Oh the anticipation is killing me! Good luck! {Emoji.four_leaf_clover}"),
            new (DayOfWeek.Saturday, $"The tournament is already going on!  You only have {{0}} left to join it! Good luck! {Emoji.four_leaf_clover}"),
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
            return string.Format(msg.Message, SpanToReadable(nextTime - now));
        }

        private record GuildMessage (DayOfWeek ByEndOf, string Message);

        private GuildMessage[] guildMessages = {
            new (DayOfWeek.Monday, "Final submissions due in {0}!"),
            new (DayOfWeek.Tuesday, "Hold on, sign up will start in {0}."),
            new (DayOfWeek.Thursday, "Sign up now by hitting the emoji in <#900801095788527616>! Signups close in {0}!"),
            new (DayOfWeek.Friday, "Signups are closed, hope you've signed up! Tournament will start in {0}."),
            new (DayOfWeek.Saturday, "Tournaments have started! {0} left to start or begin your guild group."),
        };

        [Command("guild")]
        [Summary("Tells you when the next important times are for guild-based tournaments")]
        [Usage("guild")]
        public Task Guild()
        {
            var now = DateTime.UtcNow;

            var msg = guildMessages.FirstOrDefault(m => m.ByEndOf >= now.DayOfWeek) ?? guildMessages.First();

            var daysUntil = msg.ByEndOf - now.DayOfWeek;
            if (daysUntil < 0)
            {
                daysUntil += 7;
            }
            var nextTime = now.Date.AddDays(daysUntil + 1);
            return ReplyAsync(string.Format(msg.Message, SpanToReadable(nextTime - now)));
        }

        private string SpanToReadable(TimeSpan span)
        {
            var pieces = new List<string>();
            if (span.Days > 0)
            {
                pieces.Add($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}");
            }

            if (span.Hours > 0)
            {
                pieces.Add($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")}");
            }

            if (span.Minutes > 0)
            {
                pieces.Add($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")}");
            }

            if (span.Seconds > 0)
            {
                pieces.Add($"{span.Seconds}.{span.Milliseconds:D3} second{(span.Seconds == 1 ? string.Empty : "s")}");
            }

            return pieces.Count switch {
                0 => "no time!",
                1 => pieces[0],
                2 => string.Join(" and ", pieces),
                _ => string.Join(" and ", string.Join(", ", pieces.GetRange(0, pieces.Count - 1)), pieces[^1]),
            };
        }
    }
}