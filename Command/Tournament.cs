using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace uav.Command
{
    public class Tournament : ModuleBase<SocketCommandContext>
    {
        [Command("nexttourn")]
        [Summary("Tells you when the next tournament will start")]
        public async Task NextTourn()
        {
            var now = DateTime.UtcNow;

            await ReplyAsync(NextTournReply(now));
        }

        public string NextTournReply(DateTime now)
        {
            // is there a tournament already going on?
            if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                var endOfDay = now.Date.AddDays(1);
                return $"The tournament is already going on! You only have {SpanToReadable(endOfDay - now)} left to join it!";
            }
            else
            {
                var endOfFriday = now.Date.AddDays(DayOfWeek.Saturday - now.DayOfWeek);
                if (endOfFriday < now)
                {
                    endOfFriday.AddDays(7);
                }
                var onGoing = now.DayOfWeek == DayOfWeek.Sunday ? "The tournament is going on, but you can no longer enter it.  You can join the next one.  " : string.Empty;

                return $"{onGoing}The next tournament starts in {SpanToReadable(endOfFriday - now)}.";
            }

        }

        private string SpanToReadable(TimeSpan span)
        {
            var pieces = new List<string>();
            if (span.Days > 0)
            {
                pieces.Add($"{span.Days} days");
            }

            if (span.Hours > 0)
            {
                pieces.Add($"{span.Hours} hours");
            }

            if (span.Minutes > 0)
            {
                pieces.Add($"{span.Minutes} minutes");
            }

            if (span.Seconds > 0)
            {
                pieces.Add($"{span.Seconds} seconds");
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