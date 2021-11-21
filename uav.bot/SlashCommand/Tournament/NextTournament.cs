using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;

namespace uav.bot.SlashCommand.Tournament;

public class NextTournament : BaseTournamentSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Next Tournament information");

    private record NextTournamentMessage (DayOfWeek ByEndOf, string Message, DayOfWeek? next = null);

    private static NextTournamentMessage[] nextTournamentMessages = {
        new (DayOfWeek.Thursday, $"The next tournamanet starts in {{0}}. Don't forget to sign up to a guild! Good luck! {IpmEmoji.four_leaf_clover}", DayOfWeek.Friday),
        new (DayOfWeek.Friday, $"The next tournament starts in {{0}}. Oh the anticipation is killing me! Good luck! {IpmEmoji.four_leaf_clover}"),
        new (DayOfWeek.Saturday, $"The tournament is already going on!  You only have {{0}} left to join it! Good luck! {IpmEmoji.four_leaf_clover}"),
        new (DayOfWeek.Sunday, $"The tournament is going on, but you can no longer join it. The next tournament will start in {{0}}.", DayOfWeek.Friday),
    };

    public override Task Invoke(SocketSlashCommand command)
    {
        var now = DateTimeOffset.UtcNow;
        var msg = nextTournamentMessages.FirstOrDefault(m => m.ByEndOf >= now.DayOfWeek) ?? nextTournamentMessages.First();

        var daysUntil = (msg.next ?? msg.ByEndOf) - now.DayOfWeek;
        if (daysUntil < 0)
        {
            daysUntil += 7;
        }
        var nextTime = now.Date.AddDays(daysUntil + 1);

        var message = string.Format(msg.Message, SpanToReadable(nextTime - now)) + "\n\n" + Support.SupportStatement;
        var embed = new EmbedBuilder()
            .WithTitle("Next Tournament")
            .WithDescription(message)
            .WithAuthor(command.User.ToString(), command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
            .WithColor(Color.DarkRed)
            .WithCurrentTimestamp();
        
        return command.RespondAsync(embed: embed.Build());
    }
}