using System;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using log4net;
using uav.bot.Attributes;
using uav.logic.Constants;
using uav.logic.Database;
using uav.logic.Database.Model;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public abstract class BaseSlashCommand : ISlashCommand, ICommandHandler<ComponentHandlerAttribute>
{
    protected readonly DatabaseService databaseService = new();
    protected ILog logger;

    public BaseSlashCommand()
    {
        logger = LogManager.GetLogger(GetType());
    }

    public virtual string CommandName => GetType().Name.ToSlashCommand();

    public virtual SlashCommandBuilder CommandBuilder => throw new NotImplementedException($"Missing command builder for {GetType().FullName}");
    public virtual Task<SlashCommandBuilder> CommandBuilderAsync =>
        Task.FromResult(CommandBuilder);

    public SocketSlashCommand? Command { protected get; set; }
    protected string? SubCommandName => Command?.Data.Options.First().Name;
    protected SocketInteraction Interaction =>
        Command as SocketInteraction ??
        Modal ??
        (SocketInteraction)Component!;
    
    public SocketGuildUser User => (SocketGuildUser)Interaction.User;
    private IDbUser? _dbUser;
    protected IDbUser DbUser => _dbUser ??= User.ToDbUser();

    public SocketGuild? Guild => User.Guild;

    protected string FullCommandString => Command?.Data.Name ?? Modal?.Data.CustomId ?? Component!.Data.CustomId;

    public async Task DoCommand()
    {
        // save user.
        await databaseService.SaveUser(DbUser);

        try
        {
            await Invoke();
        }
        catch (Exception e)
        {
            logger.Error($"Error occurred with command {FullCommandString}", e);
        }

        if (!isResponded)
        {
            await databaseService.AddHistory(DbUser, FullCommandString, OptionsAsString(), "No response");
        }
    }

    public virtual IEnumerable<ulong> NonEphemeralChannels => new[] {
            Channels.VacuumInSpaceAndTime,
            Channels.CreditFarmersAnonymous,
            Channels.LongHaulersGang,
    };

    public SocketModal? Modal { protected get; set; }

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
    protected async Task RespondAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool? ephemeral = null, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
    {
        var reallyEphemeral = IsEphemeral(ephemeral);
        if (isDeferred)
        {
            await Interaction.FollowupAsync(text, embeds, isTTS, reallyEphemeral, allowedMentions, components, embed, options);
        }
        else
        {
            await Interaction.RespondAsync(text, embeds, isTTS, reallyEphemeral, allowedMentions, components, embed, options);
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

#pragma warning disable IDE0060
    private async Task SaveHistory(string? text, Embed[]? embeds, bool isTTS, bool? ephemeral, AllowedMentions? allowedMentions, MessageComponent? components, Embed? embed, RequestOptions? options)
#pragma warning restore IDE0060
    {
        isResponded = true;

        var optionsString = OptionsAsString();

        var response = responses().First(x => x is not null && x.Length > 0);

        logger.Debug($"Saving history for user {DbUser.Name}, command {FullCommandString}, options {optionsString} with response {response}");
        await databaseService.AddHistory(DbUser, FullCommandString, optionsString, response);

        IEnumerable<string?> responses()
        {
            // all the options we might have as a response.
            yield return text;

            yield return embedToResponse(embed);

            yield return embedsToResponse(embeds);

            yield return "No response given";
        }

        string? embedToResponse(Embed? e)
        {
            if (e is null)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(e.Description);
            sb.AppendJoin("\n", e.Fields.Select(f => string.Join("\n", f.Name, f.Value)));

            return sb.ToString();
        }
        string? embedsToResponse(IEnumerable<Embed?>? embeds)
        {
            if (embeds is null)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendJoin("\n", embeds.Select(e => embedToResponse(e)).Where(x => x is not null));

            return sb.ToString();
        }
    }

    private string OptionsAsString()
    {
        var commandParams = new List<string>();

        AddCommandOptions(Command?.Data.Options);
        AddModalOptions(Modal?.Data);
        AddComponentOptions(Component?.Data);
        return string.Join(" ", commandParams);

        void AddCommandOptions(IEnumerable<SocketSlashCommandDataOption>? options)
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
                    _ when option.Value.ToString()!.Length == 0 => string.Empty,
                    _ => $":{option.Value}",
                };

                commandParams.Add(o);
                AddCommandOptions(option.Options);
            }
        }

        void AddModalOptions(SocketModalData? data)
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

        void AddComponentOptions(SocketMessageComponentData? data)
        {
            if (data == null)
            {
                return;
            }

            commandParams.Add($"{data.CustomId}:{data.Value}");
        }
    }

    public virtual async Task Invoke()
    {
        await Invoke(Command!);
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
        await databaseService.SaveUser(DbUser);
        try
        {
            await InvokeModal(command);
        }
        catch(Exception e)
        {
            if (!isResponded)
            {
                await RespondAsync($"Something went wrong. Please contact Tanktalus", ephemeral: true);
                var channel = await Guild!.GetUser(410138719295766537).CreateDMChannelAsync();
                // send me a private message
                await channel.SendMessageAsync(
                    $"Error occurred with modal using command {string.Join(":", command)}",
                    embed: new EmbedBuilder().WithTitle("error").WithDescription(e.ToString()).Build()
                    );
            }
        }
    }

    protected virtual Task InvokeModal(string[] command)
    {
        return Task.CompletedTask;
    }

    public SocketMessageComponent? Component { get; set; }

    public async Task DoComponent(ReadOnlyMemory<string> command)
    {
        await databaseService.SaveUser(DbUser);
        try
        {
            await InvokeComponent(command);
        }
        catch (Exception e)
        {
            if (!isResponded)
            {
                await RespondAsync($"Something went wrong. Please contact Tanktalus", ephemeral: true);
                var channel = await Guild!.GetUser(Support.SupportPerson).CreateDMChannelAsync();
                // send me a private message
                await channel.SendMessageAsync(
                    $"Error occurred with component using command {string.Join(":", command)}",
                    embed: new EmbedBuilder().WithTitle("error").WithDescription($"```\n{e}\n```").Build()
                    );
                Console.Error.WriteLine($"Error occurred with component using command {string.Join(":", command)}\n\n{e}");
            }
        }
    }

    protected virtual Task InvokeComponent(ReadOnlyMemory<string> command)
    {
        return (this as ICommandHandler<ComponentHandlerAttribute>).RunCommand(command);
    }

    protected async Task UpdateAsync(Action<MessageProperties> action)
    {
        logger.Debug($"Updating message {Component?.Message.Id}");

        if (Component is null)
        {
            throw new Exception("This is only for components");
        }

        logger.Debug($"Still updating message {Component.Message.Id}");
        await Component.UpdateAsync(action);
        logger.Debug($"Updated message {Component.Message.Id}");
        // await Component.Message.ModifyAsync(action);
    }
}

public abstract class BaseSlashCommandWithSubcommands : BaseSlashCommand
{
    public override Task Invoke(SocketSlashCommand command)
    {
        var firstOption = command.Data.Options.First();
        var options = CommandArguments(firstOption.Options);
        var subcommand = firstOption.Name;
        if (!Subcommands.TryGetValue(subcommand, out var subcommandMethod))
        {
            throw new Exception($"Subcommand {subcommand} not found");
        }

        return subcommandMethod.Invoke(options);
    }

    public abstract IDictionary<string, Func<IDictionary<string, SocketSlashCommandDataOption>, Task>>
        Subcommands { get; }

    private static MemoryCache Cache => MemoryCache.Default;

    protected ModalBuilder ModalBuilder(string command, object data)
    {
        var key = string.Join(":", new[] {command, User.Id.ToString(), Guid.NewGuid().ToString()});
        Cache.Add(key, data, DateTimeOffset.Now.AddMinutes(60));
        return ModalBuilder(key);
    }

    protected override Task InvokeModal(string[] command)
    {
        var modal = command[0];
        if (!ModalSubcommands.TryGetValue(modal, out var modalMethod))
        {
            throw new Exception($"Modal {modal} not found");
        }

        var data = Cache.Get(string.Join(":", command));
        var options = Modal!.Data.Components.ToDictionary(c => c.CustomId, c => (IComponentInteractionData)c);

        return modalMethod.Invoke(command.Skip(1).ToArray(), data, options);
    }

    protected virtual IDictionary<string, Func<string[], object, IDictionary<string, IComponentInteractionData>, Task>> ModalSubcommands { get; } = new Dictionary<string, Func<string[], object, IDictionary<string, IComponentInteractionData>, Task>>();
}