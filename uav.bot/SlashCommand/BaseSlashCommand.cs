using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Database;
using uav.logic.Database.Model;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public abstract class BaseSlashCommand : ISlashCommand
{
    protected readonly DatabaseService databaseService = new DatabaseService();

    public virtual string CommandName => GetType().Name.ToSlashCommand();

    public abstract SlashCommandBuilder CommandBuilder { get; }

    public SocketSlashCommand Command { protected get; set; }
    public SocketGuildUser User => Command.User as SocketGuildUser;
    protected IDbUser dbUser;

    public async Task DoCommand()
    {
        dbUser = User.ToDbUser();

        // save user.
        await databaseService.SaveUser(dbUser);

        try
        {
            await Invoke();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        if (!isResponded)
        {
            await databaseService.AddHistory(dbUser, commandString(), "No response");
        }
    }

    private bool isResponded = false;
    protected async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        await Command.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        isResponded = true;

        var command = commandString();

        var response = responses().First(x => x is not null && x.Length > 0);

        await databaseService.AddHistory(dbUser, command, response);

        IEnumerable<string> responses()
        {
            // all the options we might have as a response.
            yield return text;

            yield return embedToResponse(embed);

            yield return embedsToResponse(embeds);

            yield return "No response given";
        }

        string embedToResponse(Embed e)
        {
            if (e is null)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(e.Description);
            sb.AppendJoin("\n", e?.Fields.Select(f => string.Join("\n", f.Name, f.Value)));

            return sb.ToString();
        }
        string embedsToResponse(IEnumerable<Embed> embeds)
        {
            if (embeds is null)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendJoin("\n", embeds.Select(e => embedToResponse(e)));

            return sb.ToString();
        }
    }

    private string commandString()
    {
        var commandParams = new List<string> {
            Command.Data.Name,
        };

        AddOptions(Command.Data.Options);
        return string.Join(" ", commandParams);



        void AddOptions(IEnumerable<SocketSlashCommandDataOption> options)
        {
            if (options == null)
            {
                return;
            }

            foreach (var option in options)
            {
                commandParams.Add($"{option.Name}:{option.Value}");
                AddOptions(option.Options);
            }
        }

    }

    public virtual async Task Invoke()
    {
        await Invoke(Command);
    }

    public abstract Task Invoke(SocketSlashCommand command);

    protected EmbedBuilder EmbedBuilder(string title, string message, Color color)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(message)
            .WithColor(color)
            .WithCurrentTimestamp();
    }

    protected bool IsInARole(IUser user, params ulong[] requiredRoles) => IsInARole(user, (IEnumerable<ulong>) requiredRoles);

    protected bool IsInARole(params ulong[] requiredRoles) => IsInARole((IEnumerable<ulong>)requiredRoles);
    protected bool IsInARole(IEnumerable<ulong> requiredRoles) => IsInARole(Command.User, requiredRoles);

    protected bool IsInARole(IUser user, IEnumerable<ulong> requiredRoles)
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
        return CommandArguments(command.Data.Options);
    }

    protected IDictionary<string, SocketSlashCommandDataOption> CommandArguments(IEnumerable<SocketSlashCommandDataOption> options)
    {
        return options.ToDictionary(x => x.Name);
    }

}