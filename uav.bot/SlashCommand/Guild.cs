using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;

namespace uav.bot.SlashCommand;

public class Guild : BaseSlashCommand
{
    public Guild()
    {
    }

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
                .WithDescription("Get Guild-Tournament information");

    private record GuildMessage (DayOfWeek ByEndOf, string Message);

    private GuildMessage[] guildMessages = {
        new (DayOfWeek.Monday, "Final submissions due in {0}!"),
        new (DayOfWeek.Thursday, $"Sign up by hitting the emoji in <#900801095788527616>!\nIf you cannot see the channel, go to <#677924152669110292> and click on the {IpmEmoji.ipmgalaxy} reaction to see the <#900801095788527616> channel.\n\nSignups close in {{0}}!"),
        new (DayOfWeek.Friday, "Signups are closed, hope you've signed up! Tournament will start in {0}."),
        new (DayOfWeek.Saturday, "Tournaments have started! {0} left to start or begin your guild group."),
    };

    public override Task Invoke(SocketSlashCommand command)
    {
        var now = DateTime.UtcNow;

        var ephemeral = !IsInARole(command.User,
            Roles.Moderator, Roles.MinerMod, Roles.HelperMod, Roles.GuildHelper
        );

        var msg = guildMessages.FirstOrDefault(m => m.ByEndOf >= now.DayOfWeek) ?? guildMessages.First();

        var daysUntil = msg.ByEndOf - now.DayOfWeek;
        if (daysUntil < 0)
        {
            daysUntil += 7;
        }
        var nextTime = now.Date.AddDays(daysUntil + 1);

        var embed = EmbedBuilder("Tournament Guild", string.Format(msg.Message, uav.logic.Models.Tournament.SpanToReadable(nextTime - now)) + $"\n\n{Support.SupportStatement}", Color.DarkGreen);

        return RespondAsync(embed: embed.Build(), ephemeral: ephemeral);
    }

}