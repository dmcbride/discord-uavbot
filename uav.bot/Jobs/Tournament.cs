using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.Schedule;

namespace uav.bot.Jobs;

public class Tournament : Job
{
    private DiscordSocketClient _client;
    private readonly Services.Tournament _tournament;

    private SocketGuild _guild => _client.GetGuild(523911528328724502ul);

    public override string Name {
         get
         {
             return CurrentJob().Name;
         }
    }

    private record GuildJobs(DayOfWeek Day, string Name, Func<Task> Action, TimeOnly? TimeOfDay = null);
    private GuildJobs[] jobs;

    public Tournament(DiscordSocketClient client)
    {
        _client = client;
        _tournament = new Services.Tournament(_client);

        jobs = new GuildJobs[] {
            new (DayOfWeek.Sunday, "Send Reminder 1", SendReminder),
            new (DayOfWeek.Monday, "Send Reminder 2", SendReminder),
            new (DayOfWeek.Tuesday, "Closed Tournament", ClosedTournament),
            //new (DayOfWeek.Wednesday, "Start Registration", StartRegistration),
            new (DayOfWeek.Thursday, "Remind Registration", RemindRegistration),
            new (DayOfWeek.Friday, "Select Teams", SelectTeams, new TimeOnly(12,0)),
        };
    }

    private Task SelectTeams()
    {
        return _tournament.SelectTeams();
    }

    private GuildJobs CurrentJob()
    {
        var now = DateTime.UtcNow.Date;
        var nextJob = jobs.FirstOrDefault(m => m.Day > now.DayOfWeek) ?? jobs.First();

        return nextJob;
    }
    public override DateTimeOffset NextJobTime()
    {
        var now = DateTime.UtcNow;
        var nextJob = jobs.FirstOrDefault(m => m.Day > now.DayOfWeek) ?? jobs.First();

        var nextTime = now.Date.AddDays(nextJob.Day - now.DayOfWeek);
        if (nextJob.TimeOfDay != null)
        {
            nextTime.AddTicks(nextJob.TimeOfDay.Value.Ticks);
        }
        if (nextTime < now)
        {
            nextTime = nextTime.AddDays(7);
        }
        Console.WriteLine($"[{now}] Next job = {nextJob.Name} @ {nextTime.ToLocalTime()}");
        return nextTime;
    }

    public override Task Run()
    {
        var now = DateTimeOffset.UtcNow;
        var job = jobs.First(j => j.Day == now.DayOfWeek).Action;

        return job.Invoke();
    }

    // private async Task StartRegistration()
    // {
    //     // start with clearing the old roles
    //     await RemoveRoleFromEveryone(_guild.GetRole(Roles.GuildAccessRole));
    //     foreach (var teamRole in Roles.GuildTeams.Select(_guild.GetRole))
    //     {
    //         await RemoveRoleFromEveryone(teamRole);
    //     }

    //     // try removing all of the extra emotes.
    //     try {
    //         var msg = await _guild.GetTextChannel(905379297219473418ul).GetMessageAsync(900805354408009788ul);
    //         Emote.TryParse("<:tbdcapitalplanet:852848079699050527>", out var tbdcapitalplanet);
    //         var userGroups  = msg.GetReactionUsersAsync(tbdcapitalplanet, int.MaxValue);
    //         await foreach (var users in userGroups)
    //         {
    //             foreach (var user in users)
    //             {
    //                 if (!user.IsBot)
    //                 {
    //                     await msg.RemoveReactionAsync(tbdcapitalplanet, user);
    //                 }
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         log4net.LogManager.GetLogger(GetType().Name).Warn("Failed to remove reactions:", e);
    //     }
    //     finally {}

    //     // and let everyone know the signup is available.
    //     await RegistrationMessage("Tournament Guild Registration Has Begun!");
    // }

    private async Task RemindRegistration()
    {
        await RegistrationMessage("Reminder: Tournament Guild Registration");
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