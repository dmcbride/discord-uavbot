using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.Schedule;
using log4net;

namespace uav.bot.Jobs;

public class Tournament : WeeklyJobs
{
    private readonly DiscordSocketClient _client;
    private readonly Services.Tournament _tournament;

    private SocketGuild Guild => _client.GetGuild(523911528328724502ul);

    protected override ICollection<JobDescription> jobDescriptions { get; }

    private readonly ILog _logger = LogManager.GetLogger(typeof(Tournament));

    public Tournament(DiscordSocketClient client)
    {
        _client = client;
        _tournament = new Services.Tournament(_client);

        jobDescriptions = new JobDescription[] {
            // new (DayOfWeek.Sunday, "Registration Reminder 1", SendReminder),
            new (DayOfWeek.Monday, "Registration Reminder 2", SendReminder),
            new (DayOfWeek.Tuesday, "Closed Tournament", ClosedTournament),
            //new (DayOfWeek.Wednesday, "Start Registration", StartRegistration),
            new (DayOfWeek.Thursday, "Remind Registration", RemindRegistration),
            //new (DayOfWeek.Friday, "Last Warning", SendLastRegistrationReminder),
            new (DayOfWeek.Friday, "Select Teams", SelectTeams, new TimeOnly(12,0)),
        };
    }

    private bool IsNextFridayTheFirstFridayOfTheMonth()
    {
        var now = DateTime.UtcNow;
        var nextFriday = now.AddDays(DayOfWeek.Friday - now.DayOfWeek);
        return nextFriday.Day <= 7;
    }

    private bool IsLastFridayTheFirstFridayOfTheMonth()
    {
        var now = DateTime.UtcNow;
        var lastFriday = now.AddDays(DayOfWeek.Friday - now.DayOfWeek - 7);
        return lastFriday.Day <= 7;
    }

    private Task SelectTeams()
    {
        // just skip it.
        if (!IsNextFridayTheFirstFridayOfTheMonth())
        {
            return Task.CompletedTask;
        }

        // for some reason, there is a bug where this gets called on the wrong day somehow.
        // Check that it's currently Friday.
        if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Friday)
        {
            _logger.Warn("SelectTeams called on the wrong day.");
            return Task.CompletedTask;
        }

        _logger.Info("Selecting teams.");
        return _tournament.SelectTeams();
    }

    private async Task RemindRegistration()
    {
        // just skip it.
        if (!IsNextFridayTheFirstFridayOfTheMonth())
        {
            return;
        }
        await RegistrationMessage("Reminder: Tournament Guild Registration");
    }

    private async Task SendLastRegistrationReminder()
    {
        // just skip it.
        if (!IsNextFridayTheFirstFridayOfTheMonth())
        {
            return;
        }
        await RegistrationMessage("Last reminder: Tournament Guild Registration, 12 hours remain");
    }

    private async Task RegistrationMessage(string title)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(@$"⚔️ 🛡️ DISCORD GUILDS 🛡️ ⚔️ 

Come join the fun!
Looking for Miners of all levels and tournament brackets!

<#{Channels.GuildRules}>
Use the simple register via emoji option
{IpmEmoji.ipmtourney} — Full Member Access
{IpmEmoji.tbdcapitalplanet} — Allows for 1 week team play

— Get placed on {IpmEmoji.Team(1)} of up to {IpmEmoji.Team(5)} teams and fight for Top Average Rank throughout the IPM Tournament.
— The Single Requirment is posting a valid screenshot of your Final Rank Claim at the end of the tournament 
— Earn roles for Team Wins along with 💫s to mark victories 

{Support.SupportStatement}")
// TODO: get the timer working here.
// Registration closes in {0}

            .WithCurrentTimestamp()
            .WithColor(Color.Teal);
        
        await Guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(embed: embed.Build());
    }

    private async Task SendReminder()
    {
        // just skip it.
        if (!IsLastFridayTheFirstFridayOfTheMonth())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var due = now.Date.AddDays(DayOfWeek.Tuesday - now.DayOfWeek);
        var embed = new EmbedBuilder()
            .WithTitle("Guild Submissions Due")
            .WithDescription($"Final submissions due in {uav.logic.Models.Tournament.SpanToReadable(due - now)}!\n\n**You must post your own Final Rank submission!**")
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        await Guild.GetTextChannel(Channels.SubmitFinalRanksHere).SendMessageAsync(embed: embed.Build());
    }

    private async Task ClosedTournament()
    {
        // just skip it.
        if (!IsLastFridayTheFirstFridayOfTheMonth())
        {
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Guild Submissions Complete!")
            .WithDescription("Final submissions closed. Time to resolve the winner!")
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        await Guild.GetTextChannel(Channels.SubmitFinalRanksHere).SendMessageAsync(embed: embed.Build());
    }
}