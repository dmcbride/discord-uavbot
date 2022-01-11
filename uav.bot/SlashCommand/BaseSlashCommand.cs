using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand;

public abstract class BaseSlashCommand : ISlashCommand
{
    private static Regex slashCommandDashLocator = new Regex(@"(?<=[a-z])(?=[A-Z])");
    public virtual string CommandName =>
        slashCommandDashLocator.Replace(GetType().Name, "-").ToLower();

    public abstract SlashCommandBuilder CommandBuilder { get; }

    public abstract Task Invoke(SocketSlashCommand command);

    protected EmbedBuilder EmbedBuilder(IUser user, string title, string message, Color color)
    {
        return new EmbedBuilder()
            .WithAuthor(user.ToString(), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithTitle(title)
            .WithDescription(message)
            .WithColor(color)
            .WithCurrentTimestamp();
    }

    protected bool IsInARole(IUser user, params ulong[] requiredRoles)
    {
        if (!(user is SocketGuildUser _user))
        {
            return false;
        }

        var roles = requiredRoles.ToHashSet();
        return _user.Roles.Select(r => r.Id).Any(roles.Contains);
    }

    protected IDictionary<string, SocketSlashCommandDataOption> CommandArguments(SocketSlashCommand command)
    {
        return command.Data.Options.ToDictionary(x => x.Name);
    }

}