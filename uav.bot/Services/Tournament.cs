using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using uav.logic.Constants;
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

        if (users.Length < 2)
        {
            await channel.SendMessageAsync("Not enough participants to form teams");
        }

        users.Shuffle();

        int maxTeams = Math.Max(2,
            Math.Min(Roles.GuildTeams.Count, (users.Length + 4) / 5)
        );

        var teams = users
            .Select((u, i) => (u, i))
            .GroupBy(x => x.i % maxTeams)
            .Select(g => g.Select(x => x.u).OrderBy(u => u.Nickname.IsNullOrEmpty() ? u.Username : u.Nickname, StringComparer.InvariantCultureIgnoreCase).ToArray())
            .ToArray();

        var teamsMessage = string.Join("\n\n", 
            teams.Select((t, i) => $"Team {i+1} ({t.Length})\n------\n{string.Join("\n", t.Select(u => $"<@{u.Id}>"))}")
        );
        
        // hand out the roles.
        foreach (var (team, index) in teams.Select((t, i) => (t, i)))
        {
            var newRole = _guild.GetRole(Roles.GuildTeams[index]);
            foreach (var u in team)
            {
                await u.AddRoleAsync(newRole.Id);
            }
        }

        var message = $"Guild Tournament teams are:\n\n{teamsMessage}\n\nBest of luck to everyone {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!";
        await channel.SendMessageAsync(message);
        await _guild.GetTextChannel(Channels.GuildRules).SendMessageAsync(message);
        await _guild.GetTextChannel(Channels.DiscordServerNews).SendMessageAsync(message);
    }
}