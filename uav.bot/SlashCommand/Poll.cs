using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.bot.Attributes;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public class Poll : BaseSlashCommandWithSubcommands
{
    private const int maxOptions = SelectMenuBuilder.MaxOptionCount;

    public override SlashCommandBuilder CommandBuilder =>
        new SlashCommandBuilder()
            .WithDescription("UAV Poll")
            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("create")
                    .WithDescription("Create a poll")
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("id")
                            .WithDescription("Unique ID for the poll")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.String)
                    )
                    .WithType(ApplicationCommandOptionType.SubCommand)
            )
            ;

    public override IDictionary<string, Func<IDictionary<string, SocketSlashCommandDataOption>, Task>> Subcommands =>
    new Dictionary<string, Func<IDictionary<string, SocketSlashCommandDataOption>, Task>> {
            ["create"] = Create,
        };

    private async Task Create(IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var id = (string)options["id"].Value;

        var existingPoll = await databaseService.GetPollByUserKey(id, Guild.Id);
        if (existingPoll is not null)
        {
            // don't support for now.
            await RespondAsync($"A poll with ID {id} already exists. Please use a different ID.", ephemeral: true);
            return;
        }

        await Create(id);
    }

    private async Task Create(string id, string createTitleSuffix = "", string defaultTitle = "", string defaultDescription = "", string defaultDuration = "7", string defaultMaxOptions = "", string defaultOptions = "")
    {
        var mb = ModalBuilder("create", id)
            .WithTitle($"Create Poll: {id}{createTitleSuffix}")
            .AddTextInput("Poll Question", "description", value: defaultDescription)
            .AddTextInput("Poll Duration (in days)", "length", value: defaultDuration)
            .AddTextInput("Max options users can select", "max-options", value: defaultMaxOptions)
            .AddTextInput($"Options (one per line) (Max: {maxOptions})", "options", TextInputStyle.Paragraph, value: defaultOptions)
            //.AddComponents()
            ;
        
        await RespondAsync(mb);
    }

    protected override Dictionary<string, Func<string[], object, IDictionary<string, IComponentInteractionData>, Task>> ModalSubcommands => new () {
        ["create"] = CreateModal,
    };

    private async Task CreateModal(string[] args, object data, IDictionary<string, IComponentInteractionData> options)
    {
        var pollUserKey = (string)data;
        if (pollUserKey is null)
        {
            await RespondAsync("I'm sorry, I lost track of what you were talking about. Please try again.");
            return;
        }

        var description = options["description"].Value;
        var length = options["length"].Value;
        var optionsText = options["options"].Value;
        var maxOptionsString = options["max-options"].Value;
        var error = "";

        if (!double.TryParse(length, out var lengthInDays))
        {
            error += " (Invalid length)";
        }
        var endDate = DateTimeOffset.UtcNow.AddDays(lengthInDays);

        var acceptableOptions = true;
        if (!int.TryParse(maxOptionsString, out var maxOptions) || maxOptions < 1)
        {
            maxOptionsString = "";
            acceptableOptions = false;
            error += " (Invalid Max Options)";
        }

        var optionsList = optionsText
            .Split(Environment.NewLine)
            .Where(o => o.HasValue())
            .Select((o, i) => new uav.logic.Database.Model.Poll.OptionsType.Option(i, o))
            .ToList();

        if (optionsList.Count == 0)
        {
            error += " (No Options)";
        }
        else if (optionsList.Count < maxOptions && acceptableOptions)
        {
            error += " (Max Options > Options)";
        }

        if (error != "")
        {
            await RespondAsync($"Error: {error}");
            return;
        }

        var poll = new uav.logic.Database.Model.Poll
        {
            PollUserKey = pollUserKey,
            Description = description,
            EndDate = endDate,
            MaxOptions = maxOptions,
            Options = new uav.logic.Database.Model.Poll.OptionsType{
                Options = optionsList,
            },
            GuildId = Modal.GuildId ?? 0,
        };

        await databaseService.CreatePoll(poll);

        var restUserMessage = await Modal.Channel.SendMessageAsync(embed: poll.ToEmbedded().Build(), components: poll.ToSelectMenu(CommandName));
        poll.MsgId = restUserMessage.Id;
        poll.ChannelId = restUserMessage.Channel.Id;
        await databaseService.UpdatePoll(poll);

        await RespondAsync($"Poll {poll.PollUserKey} created", ephemeral: true);
    }

    [ComponentHandler("vote")]
    private async Task Vote(ReadOnlyMemory<string> options)
    {
        if (!ulong.TryParse(options.Span[0], out var pollId))
        {
            await RespondAsync("Invalid poll id", ephemeral: true);
            return;
        }
        var poll = await databaseService.GetPoll(pollId, Guild.Id);
        if (poll == null)
        {
            await RespondAsync("Poll not found", ephemeral: true);
            return;
        }
        var userId = Interaction.User.Id;
        var values = Component.Data.Values;
        var pollValues = poll.Options.Options.ToDictionary(o => o.Id.ToString(), o => o);
        var userSelectedValues = values.NaturalOrderBy(v => v).Select(v => pollValues[v].Text).ToList();
        var votedPreviously = await databaseService.VotePoll(poll, userId, values);
        
        var response = votedPreviously switch {
            true => $"Replaced your old vote with your new vote: {string.Join(",", userSelectedValues)}. You can now dismiss this message.",
            false => $"Registered your vote for {string.Join(", ", userSelectedValues)}. You can now dismiss this message.",
        };

        await RespondAsync(response, ephemeral: true);
    }

    [ComponentHandler("results")]
    private async Task Results(ReadOnlyMemory<string> options)
    {
        if (!ulong.TryParse(options.Span[0], out var pollId))
        {
            await RespondAsync("Invalid poll id", ephemeral: true);
            return;
        }
        var poll = await databaseService.GetPoll(pollId, Guild.Id);
        if (poll == null)
        {
            await RespondAsync("Poll not found", ephemeral: true);
            return;
        }
        var userId = Interaction.User.Id;
        var pollNames = poll.Options.Options.ToDictionary(o => o.Id, o => o.Text);

        var results = await databaseService.GetDetailedPollResults(poll.PollId);
        var embed = new EmbedBuilder()
            .WithDescription(poll.Description)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            ;
        foreach (var r in results)
        {
            embed.AddField(pollNames[r.vote], string.Join("\n", r.users.Select(u => u.User_Nick ?? u.User_Name)));
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true);
   }

}