using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;
using IpmEmoji = uav.logic.Constants.IpmEmoji;

namespace uav.bot.Services;

public class Tournament
{
    private static Emoji[] TeamEmojis = new[] {
        Emoji.Parse("1️⃣"),
        Emoji.Parse("2️⃣"),
        Emoji.Parse("3️⃣"),
        Emoji.Parse("4️⃣"),
        Emoji.Parse("5️⃣"),
        Emoji.Parse("6️⃣"),
        Emoji.Parse("7️⃣"),
        Emoji.Parse("8️⃣"),
        Emoji.Parse("9️⃣"),
        Emoji.Parse("🔟"),
    };

    public Tournament(DiscordSocketClient client)
    {
        _guild = client.GetGuild(523911528328724502ul);
    }

    public Tournament(SocketGuild guild)
    {
        _guild = guild;
    }

    private SocketGuild _guild;

    public IEnumerable<(SocketGuildUser user, bool permanent)> TournamentContestants()
    {
        var guildAccessRole = _guild.GetRole(Roles.GuildAccessRole);
        var permanentGuildRole = _guild.GetRole(Roles.PermanentGuildAccessRole);
        return permanentGuildRole.Members.Select(m => (user: m, permanent: true))
            .Concat(guildAccessRole.Members.Select(m => (user: m, permanent: false)))
            .DistinctBy(m => m.user.Id);
    }

    public async Task SelectTeams()
    {
        var users = TournamentContestants().Select(c => c.user).ToArray();

        var channel = _guild.GetTextChannel(Channels.AllTeamsRallyRoom);
        if (!users.Any())
        {
            await channel.SendMessageAsync("Guild tournament cancelled: No contestants found.");
            return;
        }

        if (users.Length < 2)
        {
            await channel.SendMessageAsync("Guild tournament cancelled: Not enough participants to form teams.");
            return;
        }

        users.Shuffle();

        var maxTeams = Math.Max(2,
            Math.Min(Roles.GuildTeams.Count, (users.Length + 4) / 5)
        );

        var teams = users
            .Select((u, i) => (u, i))
            .GroupBy(x => x.i % maxTeams)
            .Select(g => g.Select(x => x.u).OrderBy(u => u.Nickname.IsNullOrEmpty() ? u.Username : u.Nickname, StringComparer.InvariantCultureIgnoreCase).ToArray())
            .ToArray();

        var teamsMessage = string.Join("\n\n", 
            teams.Select((t, i) => $"Team {i+1} ({t.Length} members)\n------\n{string.Join("\n", t.Select(u => u.Mention))}")
        );
        var teamsEmbeds = teams.Select((t, i) =>
            new EmbedFieldBuilder()
                .WithName($"Team {i+1}")
                .WithValue(string.Join("\n", t.Select(u => u.Mention)))
                .WithIsInline(true)
        ).AndThen(new EmbedFieldBuilder().WithName("Vote for the winner!").WithValue("Which team do you think will win? Add your reaction, see who gets it right!").WithIsInline(false));

        // hand out the roles.
        foreach (var (team, index) in teams.Select((t, i) => (t, i)))
        {
            var newRole = _guild.GetRole(Roles.GuildTeams[index]);
            foreach (var u in team)
            {
                await u.AddRoleAsync(newRole.Id);
            }
        }

        // guild rules gets the notification message.
        var message = $"Guild Tournament teams are:\n\n{teamsMessage}\n\nBest of luck to everyone {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!\n\n{Support.SupportStatement}";
        var guildRulesChannel = _guild.GetTextChannel(Channels.GuildRules);
        await guildRulesChannel.SendMessageAsync(message);

        // discord server gets non-notification.
        var nonNotificationMessage = $"Guild Tournament teams have been selected.\n\nBest of luck to everyone {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!\n\n{Support.SupportStatement}";
        var nonNotificationEmbed = new EmbedBuilder()
            .WithTitle("Guild Tournament Teams Selected!")
            .WithDescription(nonNotificationMessage)
            .WithFields(teamsEmbeds)
            .WithColor(Color.Blue)
            .Build();
        var discordServerNewsChannel = _guild.GetTextChannel(Channels.DiscordServerNews);
        var msg = await discordServerNewsChannel.SendMessageAsync(embed: nonNotificationEmbed);

        var reactions = TeamEmojis.Take(maxTeams).ToArray();
        await msg.AddReactionsAsync(reactions);
    }
}