using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.Schedule;

namespace uav.bot.Jobs;

public class Tournament : WeeklyJobs
{
    private DiscordSocketClient _client;
    private readonly Services.Tournament _tournament;

    private SocketGuild _guild => _client.GetGuild(523911528328724502ul);

    protected override ICollection<JobDescription> jobDescriptions { get; }

    public Tournament(DiscordSocketClient client)
    {
        _client = client;
        _tournament = new Services.Tournament(_client);

        jobDescriptions = new JobDescription[] {
            new (DayOfWeek.Sunday, "Registration Reminder 1", SendReminder),
            new (DayOfWeek.Monday, "Registration Reminder 2", SendReminder),
            new (DayOfWeek.Tuesday, "Closed Tournament", ClosedTournament),
            //new (DayOfWeek.Wednesday, "Start Registration", StartRegistration),
            new (DayOfWeek.Thursday, "Remind Registration", RemindRegistration),
            new (DayOfWeek.Friday, "Last Warning", SendLastRegistrationReminder),
            new (DayOfWeek.Friday, "Select Teams", SelectTeams, new TimeOnly(12,0)),
            new (DayOfWeek.Saturday, "Checkin Reminder 1", CheckInReminder),
            new (DayOfWeek.Saturday, "Checkin Reminder 2", CheckInReminder, new TimeOnly(12, 0)),
        };
    }

    private Task SelectTeams()
    {
        return _tournament.SelectTeams();
    }

    private Task CheckInReminder()
    {
        return _tournament.CheckInNotification();
    }

    private async Task RemindRegistration()
    {
        await RegistrationMessage("Reminder: Tournament Guild Registration");
    }

    private async Task SendLastRegistrationReminder()
    {
        await RegistrationMessage("Last reminder: Tournament Guild Registration, 12 hours remain");
    }

    private async Task RegistrationMessage(string title)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(@$"Guild Event!
<#900801095788527616>

Anyone that plans on competing in the upcoming tournament is eligible to sign up. Teams are determined 24 hours before the start of the tournament.

Come and join the fun!

If you cannot see the channel, go to <#677924152669110292> and click on the {IpmEmoji.ipmgalaxy} reaction to see the <#900801095788527616> channel.

{Support.SupportStatement}")
            .WithCurrentTimestamp()
            .WithColor(Color.Teal);
        
        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(embed: embed.Build());
    }

    private async Task RemoveRoleFromEveryone(SocketRole role)
    {
        foreach (var user in role.Members)
        {
            await user.RemoveRoleAsync(role.Id);
        }
    }

 
    private async Task SendReminder()
    {
        var now = DateTime.UtcNow;
        var due = now.Date.AddDays(DayOfWeek.Tuesday - now.DayOfWeek);
        var embed = new EmbedBuilder()
            .WithTitle("Guild Submissions Due")
            .WithDescription($"Final submissions due in {uav.logic.Models.Tournament.SpanToReadable(due - now)}!\n\n**You must post your own Final Rank submission!**")
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        await _guild.GetTextChannel(Channels.SubmitFinalRanksHere).SendMessageAsync(embed: embed.Build());
    }

    private async Task ClosedTournament()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Guild Submissions Complete!")
            .WithDescription("Final submissions closed. Time to resolve the winner!")
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        await _guild.GetTextChannel(Channels.SubmitFinalRanksHere).SendMessageAsync(embed: embed.Build());
    }
}