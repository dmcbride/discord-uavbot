using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Extensions;
using IpmEmoji = uav.logic.Constants.IpmEmoji;

namespace uav.bot.Services;

public class Tournament
{
    public Tournament(DiscordSocketClient client)
    {
        _client = client;
    }

    private DiscordSocketClient _client { get; }
    private SocketGuild _guild => _client.GetGuild(523911528328724502ul);

    private static class Channels
    {
        public static ulong AllTeamsRallyRoom = 899407911900573716ul;
        public static ulong GuildRules = 900801095788527616ul;

        public static ulong SubmitFinalRanksHere = 903046199765000202ul;
        public static ulong DiscordServerNews = 909548441040986173ul;
    }

    private static class Roles
    {
        public static ulong GuildAccessRole = 876377705501827082ul;
        public static ulong[] GuildTeams = new[] {
            900814071853613068ul, // team 1
            902712361838841936ul, // team 2
            902712424493383740ul, // team 3
        };
    }

    public async Task SelectTeams()
    {
        var role = _guild.GetRole(876377705501827082L); // Guild Access Role
        var users = role.Members.ToArray();

        var channel = _guild.GetTextChannel(Channels.AllTeamsRallyRoom);
        if (!_guild.HasAllMembers)
        {
            await channel.SendMessageAsync("Still downloading users ... this can take a while.");
        }

        if (!users.Any())
        {
            await channel.SendMessageAsync("No members found?");
            return;
        }

        var tax = PreparationTax();

        users.Shuffle();

        var teams = users
            .Select((u, i) => (u, i))
            .GroupBy(x => x.i % 3)
            .Select(g => g.Select(x => x.u).OrderBy(u => u.Nickname.IsNullOrEmpty() ? u.Username : u.Nickname, StringComparer.InvariantCultureIgnoreCase).ToArray())
            .ToArray();

        var teamsMessage = string.Join("\n\n", 
            teams.Select((t, i) => $"Team {i+1}\n------\n{string.Join("\n", t.Select(u => $"<@{u.Id}>"))}")
        );
        
        await tax;

        // hand out the roles.
        foreach (var (team, index) in teams.Select((t, i) => (t, i)))
        {
            var newRole = _guild.GetRole(Roles.GuildTeams[index]);
            foreach (var u in team)
            {
                await u.AddRoleAsync(newRole.Id);
            }
        }

        var message = $"Teams are:\n\n{teamsMessage}\nBest of luck to everyone {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!";
        await channel.SendMessageAsync(message);
        await _guild.GetTextChannel(Channels.GuildRules).SendMessageAsync(message);
        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(message);

        async Task PreparationTax()
        {
            await channel.SendMessageAsync("Counting users...");
            await Task.Delay(550);
            await channel.SendMessageAsync("Sorting...");
            await Task.Delay(450);
            await channel.SendMessageAsync("(The anticipation is killing me!)");
            await Task.Delay(350);
            await channel.SendMessageAsync("Increasing quantisation of randomness....");
            await Task.Delay(250);
            await channel.SendMessageAsync("I think I got it. Double checking.");
            await Task.Delay(175);
            await channel.SendMessageAsync("Got it!");
        }
    }
}