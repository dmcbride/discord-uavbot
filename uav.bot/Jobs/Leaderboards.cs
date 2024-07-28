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
        _client = client;

        jobDescriptions = [
            new (10, "Leaderboard Reminder", SendLeaderboardReminder),
            new (20, "Leaderboard Reminder", SendLeaderboardReminder),
            new (-3, "Newsletter Reminder", SendNewsletterReminder),
            // this is UTC midnight the day before the end of the month - which is roughly 48 hours before end of month. A bit confusing.
            new (-1, "Leaderboard Final Reminder", SendLeaderboardFinalReminder),
        ];
    }

    private async Task SendNewsletterReminder()
    {
        // we want to give them until two days before the end of the month to submit their newsletter submissions.
        var now = DateTimeOffset.UtcNow;
        var finalDayToSubmit = DateTime.DaysInMonth(now.Year, now.Month) - 2;

        var finalDaySuffix = (finalDayToSubmit % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };

        var embed = new EmbedBuilder()
            .WithTitle("Newsletter Reminder")
            .WithDescription(@$"The newsletter is planned to be compiled soon. If you have anything you'd like to include, please submit it in <#{Channels.RoleClaims}> by the {finalDayToSubmit}{finalDaySuffix}. Thanks!")
            .WithColor(Color.DarkRed);

        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(embed: embed.Build());
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
