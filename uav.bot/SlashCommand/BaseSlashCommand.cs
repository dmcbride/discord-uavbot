using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Database;
using uav.logic.Database.Model;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public abstract class BaseSlashCommand : ISlashCommand
{
    protected readonly DatabaseService databaseService = new DatabaseService();

    public virtual string CommandName => GetType().Name.ToSlashCommand();

    public virtual SlashCommandBuilder CommandBuilder => throw new NotImplementedException($"Missing command builder for {GetType().FullName}");
    public virtual Task<SlashCommandBuilder> CommandBuilderAsync =>
        Task.FromResult(CommandBuilder);

    public SocketSlashCommand Command { protected get; set; }
    protected string SubCommandName => Command.Data.Options.First().Name;
    protected SocketInteraction Interaction => Command as SocketInteraction ?? Modal;
    
    public SocketGuildUser User => Interaction.User as SocketGuildUser;
    private IDbUser _dbUser;
    protected IDbUser dbUser => _dbUser ??= User.ToDbUser();

    public SocketGuild Guild => User.Guild;

    public async Task DoCommand()
    {
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
            await databaseService.AddHistory(dbUser, commandString(), null, "No response");
        }
    }

    public virtual IEnumerable<ulong> NonEphemeralChannels => new[] {
            Channels.VacuumInSpaceAndTime,
            Channels.CreditFarmersAnonymous,
            Channels.LongHaulersGang,
    };

    public SocketModal Modal { protected get; set; }

    private bool isDeferred = false;
    protected async Task DeferAsync(bool? ephemeral = null)
    {
        if (Command is not null)
        {
            ephemeral ??= IsEphemeral(ephemeral);
            isDeferred = true;
            await Command.DeferAsync(ephemeral.Value);
        }
    }

    protected virtual bool IsEphemeral(bool? ephemeral)
    {
        return ephemeral ?? !NonEphemeralChannels.Contains(Interaction.Channel.Id);
    }

    private bool isResponded = false;
    protected async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool? ephemeral = null, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        var reallyEphemeral = IsEphemeral(ephemeral);
        if (isDeferred)
        {
            await Interaction.FollowupAsync(text, embeds, isTTS, reallyEphemeral, allowedMentions, components, embed, options);
        }
        else
        {
            await (Interaction).RespondAsync(text, embeds, isTTS, reallyEphemeral, allowedMentions, components, embed, options);
        }        
        await SaveHistory(text, embeds, isTTS, reallyEphemeral, allowedMentions, components, embed, options);
    }

    protected async Task RespondAsync(ModalBuilder modal)
    {
        await Interaction.RespondWithModalAsync(modal.Build());
        await SaveHistory("Responded with modal", null, false, false, null, null, null, null);
    }

    protected ModalBuilder ModalBuilder(params string[] id) => new ModalBuilder()
        .WithCustomId(string.Join(":", CommandName.AndThen(id)));

    private async Task SaveHistory(string text, Embed[] embeds, bool isTTS, bool? ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
    {
        isResponded = true;

        var command = commandString();

        var response = responses().First(x => x is not null && x.Length > 0);

        await databaseService.AddHistory(dbUser, Command?.Data.Name ?? Modal.Data.CustomId, command, response);

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
        var commandParams = new List<string>();

        AddCommandOptions(Command?.Data.Options);
        AddModalOptions(Modal?.Data);
        return string.Join(" ", commandParams);

        void AddCommandOptions(IEnumerable<SocketSlashCommandDataOption> options)
        {
            if (options == null)
            {
                return;
            }

            foreach (var option in options)
            {
                var o = option.Name + option.Value switch {
                    null => string.Empty,
                    IAttachment a => $":{a.Url}",
                    SocketGuildUser sgu => $":@{sgu.DisplayName()}",
                    _ when option.Value.ToString().Length == 0 => string.Empty,
                    _ => $":{option.Value}",
                };

                commandParams.Add(o);
                AddCommandOptions(option.Options);
            }
        }

        void AddModalOptions(SocketModalData data)
        {
            if (data == null)
            {
                return;
            }

            foreach (var option in data.Components)
            {
                commandParams.Add($"{option.CustomId}:{option.Value}");
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
            .WithColor(color);
    }

    protected bool IsInARole(params ulong[] requiredRoles) => IsInARole((IEnumerable<ulong>)requiredRoles);
    protected bool IsInARole(IEnumerable<ulong> requiredRoles, params ulong[] moreRoles) => IsInARole(requiredRoles.AndThen(moreRoles).ToHashSet());
    protected bool IsInARole(ISet<ulong> requiredRoles)
    {
        if (Interaction.User is not SocketGuildUser _user)
        {
            return false;
        }

        return _user.Roles.Select(r => r.Id).Any(requiredRoles.Contains);
    }

    protected IDictionary<string, SocketSlashCommandDataOption> CommandArguments(SocketSlashCommand command)
    {
        return CommandArguments(command.Data.Options);
    }

    protected IDictionary<string, SocketSlashCommandDataOption> CommandArguments(IEnumerable<SocketSlashCommandDataOption> options)
    {
        return options.ToDictionary(x => x.Name);
    }

    public async Task DoModal(string[] command)
    {
        await databaseService.SaveUser(dbUser);
        await InvokeModal(command);
    }

    protected virtual Task InvokeModal(string[] command)
    {
        return Task.CompletedTask;
    }
}