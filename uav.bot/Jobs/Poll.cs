using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Database;
using uav.Schedule;

namespace uav.bot.Jobs;

public class Poll : Job
{
    private readonly DatabaseService _databaseService = new DatabaseService();
    private readonly DiscordSocketClient _client;
    private logic.Database.Model.Poll? _nextPoll;

    public Poll(DiscordSocketClient client)
    {
        _client = client;
    }

    public override string Name => "Poll";

    public override async Task<DateTimeOffset?> NextJobTime()
    {
        _nextPoll = await _databaseService.GetNextExpiringPoll();
        Console.WriteLine($"Next poll: {_nextPoll?.PollId} @ {_nextPoll?.EndDate} {_nextPoll?.EndDate.ToLocalTime()}");
        var next = _nextPoll?.EndDate ?? DateTimeOffset.UtcNow.AddHours(1);
        if (next < DateTimeOffset.UtcNow)
        {
            next = DateTimeOffset.UtcNow.AddSeconds(10);
        }
        return next;
    }

    public override async Task Run()
    {
        if (_nextPoll == null || _nextPoll.Completed)
        {
            return;
        }

        var pollResults = await _databaseService.GetPollResults(_nextPoll.PollId);
        var usersParticipating = await _databaseService.GetNumberOfUsersVotingFor(_nextPoll.PollId);
        var pollValues = _nextPoll.Options.Options.ToDictionary(o => o.Id, o => o.Text);
        await _databaseService.CompletePoll(_nextPoll);

        await _client.GetGuild(_nextPoll.GuildId).GetTextChannel(_nextPoll.ChannelId).ModifyMessageAsync(
            _nextPoll.MsgId,
            p => {
                p.Embed = new Discord.EmbedBuilder()
                    .WithFooter($"Poll ID: {_nextPoll.PollUserKey} (poll {_nextPoll.PollId})")
                    .WithTimestamp(_nextPoll.EndDate)
                    .WithDescription(_nextPoll.Description)
                    .WithColor(Discord.Color.Red)
                    .AddField("Number of Users Participating", usersParticipating, true)
                    .AddField("Results", $"```{GetPollResultsString(pollResults, pollValues)}\n```", false)
                    .Build();
                // add in a button to retrieve the detailed results
                p.Components = Optional.Create<Discord.MessageComponent>(
                    new ComponentBuilder()
                        .WithButton(new ButtonBuilder()
                            .WithLabel("Detailed Results")
                            .WithCustomId($"poll:results:{_nextPoll.PollId}")
                            .WithStyle(ButtonStyle.Primary)
                        )
                        .Build()
                );
            });

        // send a message to the channel with the detailed results
        await _client.GetGuild(_nextPoll.GuildId).GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(
            @$"Poll ID ""{_nextPoll.PollUserKey}"" has ended! If you'd like to look at past polls or vote in open polls check out {MentionUtils.MentionChannel(_nextPoll.ChannelId)} There you can also click on ""Detailed Results"" to see the breakdown of how everyone voted.",
            embed: new Discord.EmbedBuilder()
                .WithFooter($"Poll ID: {_nextPoll.PollUserKey} (poll {_nextPoll.PollId})")
                .WithTimestamp(_nextPoll.EndDate)
                .WithDescription(_nextPoll.Description)
                .WithColor(Discord.Color.Red)
                .AddField("Number of Users Participating", usersParticipating, true)
                .AddField("Results", $"```{GetPollResultsString(pollResults, pollValues)}\n```", false)
                .Build()
        );
    }

    private static string GetPollResultsString(IEnumerable<DatabaseService.PollResults> pollResults, IDictionary<int, string> pollValues)
    {
        var results = new List<string>();
        var optionLength = pollValues.Values.Select(v => v.Length).Max();

        foreach (var r in pollResults.OrderBy(r => r.Vote))
        {
            results.Add($"{pollValues[r.Vote].PadRight(optionLength)} | {r.Count}");
        }
        return string.Join("\n", results);
    }
}