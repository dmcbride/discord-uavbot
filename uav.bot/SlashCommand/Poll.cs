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
            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("view")
                    .WithDescription("View a poll")
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("id")
                            .WithDescription("Unique ID for the poll")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.String)
                    )
                    .WithType(ApplicationCommandOptionType.SubCommand)
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("voted-in")
                    .WithDescription("How many polls has a user voted in?")
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("user")
                            .WithDescription("User")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.User)
                    )
                    .WithType(ApplicationCommandOptionType.SubCommand)
            )
            ;

    public override IDictionary<string, Func<IDictionary<string, SocketSlashCommandDataOption>, Task>> Subcommands =>
    new Dictionary<string, Func<IDictionary<string, SocketSlashCommandDataOption>, Task>> {
            ["create"] = Create,
            ["view"] = View,
            ["voted-in"] = VotedIn,
        };

    private async Task Create(IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var id = (string)options["id"].Value;

        var existingPoll = await databaseService.GetPollByUserKey(id, Guild!.Id);
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
            ;
        
        await RespondAsync(mb);
    }

    private async Task View(IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var id = (string)options["id"].Value;

        var poll = await databaseService.GetPollByUserKey(id, Guild!.Id);
        if (poll is null)
        {
            await RespondAsync($"Poll {id} not found", ephemeral: true);
            return;
        }

        var details = await databaseService.GetDetailedPollResults(poll.PollId);
        var usersVoted = details.SelectMany(d => d.users).DistinctBy(u => u.User_Id).ToArray();
        var totalVotes = details.Sum(d => d.users.Length);

        await RespondAsync(
            embed: new EmbedBuilder()
                .WithTitle($"Poll: {poll.Description}")
                .WithDescription($"Poll ID: {poll.PollUserKey}")
                .AddField("End Date", poll.EndDate.ToString("yyyy-MM-dd HH:mm:ss zzz"))
                .AddField("Total Votes", totalVotes)
                .AddField($"{usersVoted.Length} Users Voted", string.Join(Environment.NewLine, usersVoted.Select(u => u.Name())))
                .Build(),
            ephemeral: true
        );
    }

    private async Task VotedIn(IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var user = (IGuildUser)options["user"].Value;

        var pollsVotedIn = await databaseService.GetNumberOfPollsUserVotedIn(user.Id, Guild!.Id);

        await RespondAsync(
            embed: new EmbedBuilder()
                .WithTitle($"Polls Voted In: {user.Nickname ?? user.Username}")
                .AddField("Polls Voted In", pollsVotedIn)
                .Build(),
            ephemeral: true
        );
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
            GuildId = Modal!.GuildId ?? 0,
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
        var poll = await databaseService.GetPoll(pollId, Guild!.Id);
        if (poll == null)
        {
            await RespondAsync("Poll not found", ephemeral: true);
            return;
        }
        var userId = Interaction.User.Id;
        var values = Component!.Data.Values;
        var pollValues = poll.Options.Options.ToDictionary(o => o.Id.ToString(), o => o);
        var userSelectedValues = values.NaturalOrderBy(v => v).Select(v => pollValues[v].Text).ToList();
        var voteResults = await databaseService.VotePoll(poll, userId, values);
        
        var response = voteResults.votedPreviously switch {
            true => $"Replaced your old vote with your new vote: {string.Join(",", userSelectedValues)}. You can now dismiss this message.",
            false => $"Registered your vote for {string.Join(", ", userSelectedValues)}. You can now dismiss this message.",
        };

        await RespondAsync(response, ephemeral: true);

        // update the message
        await Interaction.Channel.ModifyMessageAsync(poll.MsgId, p => {
           p.Embed = poll.ToEmbedded(voteResults.voteCount).Build();
        });
    }

    [ComponentHandler("results")]
    private async Task Results(ReadOnlyMemory<string> options)
    {
        if (!ulong.TryParse(options.Span[0], out var pollId))
        {
            await RespondAsync("Invalid poll id", ephemeral: true);
            return;
        }
        var poll = await databaseService.GetPoll(pollId, Guild!.Id);
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