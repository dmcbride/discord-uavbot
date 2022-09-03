using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.Schedule;

namespace uav.bot.Jobs;

public class Leaderboards : MonthlyJobs
{
    private readonly DiscordSocketClient _client;
    private SocketGuild _guild => _client.GetGuild(523911528328724502ul);

    public Leaderboards(DiscordSocketClient client)
    {
        this._client = client;

        jobDescriptions = new JobDescription[] {
            new (10, "Leaderboard Reminder", SendLeaderboardReminder),
            new (20, "Leaderboard Reminder", SendLeaderboardReminder),
            // this is UTC midnight the day before the end of the month - which is roughly 48 hours before end of month. A bit confusing.
            new (-1, "Leaderboard Final Reminder", SendLeaderboardFinalReminder),
        };
    }

    protected override ICollection<JobDescription> jobDescriptions { get; }

    private async Task SendLeaderboardReminder()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Galaxy Leaderboards Reminder")
            .WithDescription(@$"There are top 20 records to pursue across the following: <#{Channels.Records.SingleGalaxyRecords}> <#{Channels.Records.LegacyGalaxyRecords}> <#{Channels.Records.ChallengeRecords}> <#{Channels.Records.TournamentRecords}> <#{Channels.Records.SpecialtyRecords}>

Each spot gets you points, each month those points are totaled up into tiers and can get you a role: <#{Channels.Records.OverallRankings}>

See <#{Channels.Records.OverviewAndClaim}> to get started and get your claims in!")
            .WithColor(Color.DarkRed);
        
        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(embed: embed.Build());
    }

    private async Task SendLeaderboardFinalReminder()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Reminder for Galaxy Leaderboards")
            .WithDescription(@$"-Point tabulation, ranking update, and role reassignment for <#{Channels.Records.OverallRankings}> is planned for around the first day of the month.

-Any top 20 record claims you want included for this month of rankings, please submit no later than 24 hours from now in <#{Channels.Records.OverviewAndClaim}>.");

        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(embed: embed.Build());
    }
}
