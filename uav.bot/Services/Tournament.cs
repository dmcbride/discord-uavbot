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

    private static Emoji[] GoTeamEmojis = new[] {
        Emoji.Parse("🇬"),
        Emoji.Parse("🇴"),
        Emoji.Parse("🇹"),
        Emoji.Parse("🇪"),
        Emoji.Parse("🇦"),
        Emoji.Parse("🇲"),
    };

    public Tournament(DiscordSocketClient client)
    {
        _client = client;
    }

    public Tournament(SocketGuild guild)
    {
        _guild = guild.ValidateNotNull(nameof(guild));
    }

    private SocketGuild _guild;
    private SocketGuild Guild =>
        _guild ??= _client.GetGuild(523911528328724502ul);

    private readonly DiscordSocketClient _client;

    public IEnumerable<(SocketGuildUser user, bool permanent)> TournamentContestants()
    {
        var guildAccessRole = Guild.GetRole(Roles.GuildAccessRole);
        var permanentGuildRole = Guild.GetRole(Roles.PermanentGuildAccessRole);
        return permanentGuildRole.Members.Select(m => (user: m, permanent: true))
            .Concat(guildAccessRole.Members.Select(m => (user: m, permanent: false)))
            .DistinctBy(m => m.user.Id);
    }

    public async Task SelectTeams()
    {
        var users = TournamentContestants().Select(c => c.user).ToArray();

        var channel = Guild.GetTextChannel(Channels.AllTeamsRallyRoom);
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

        var teamsMessages = 
            teams.Select((t, i) => $"Team {i+1} ({t.Length} members)\n------\n{string.Join("\n", t.Select(u => u.Mention))}");
        var teamsEmbeds = teams.Select((t, i) =>
            new EmbedFieldBuilder()
                .WithName($"Team {i+1} ({t.Length})")
                .WithValue(string.Join("\n", t.Select(u => u.DisplayName())))
                .WithIsInline(true)
        ).AndThen(new EmbedFieldBuilder().WithName("Vote for the winner!").WithValue("Which team do you think will win? Add your reaction, see who gets it right!").WithIsInline(false));

        // hand out the roles.
        foreach (var (team, index) in teams.Select((t, i) => (t, i)))
        {
            var newRole = Guild.GetRole(Roles.GuildTeams[index]);
            foreach (var u in team)
            {
                await u.AddRoleAsync(newRole.Id);
            }
        }

        // discord server gets non-notification.
        var nonNotificationMessage = $"Guild Tournament teams have been selected.\n\nBest of luck to everyone {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!\n\n{Support.SupportStatement}";
        var nonNotificationEmbed = new EmbedBuilder()
            .WithTitle("Guild Tournament Teams Selected!")
            .WithDescription(nonNotificationMessage)
            .WithFields(teamsEmbeds)
            .WithColor(Color.Blue)
            .Build();
        var discordServerNewsChannel = Guild.GetTextChannel(Channels.DiscordServerNews);
        var msg = await discordServerNewsChannel.SendMessageAsync(embed: nonNotificationEmbed);

        var reactions = TeamEmojis.Take(maxTeams).ToArray();
        await msg.AddReactionsAsync(reactions);

        // team channels get the notification messages.        
        foreach (var (t, i) in teams.Select((msg, i) => (msg, i)))
        {
            var teamChannel = Guild.GetTextChannel(Channels.TeamChannels[i]);
            var message = $"Guild team {i+1} ({t.Length} members)\n-------\n{string.Join("\n", t.Select(u => u.Mention))}\n\nBest of luck to team {i+1} {IpmEmoji.four_leaf_clover} and may the markets be ever in your favour!";
            msg = await teamChannel.SendMessageAsync(message);
            reactions = GoTeamEmojis.AndThen(TeamEmojis[i]).ToArray();
            await msg.AddReactionsAsync(reactions);

            await Task.Delay(5000);
        }
    }

    public async Task CheckInNotification()
    {
        var channel = Guild.GetTextChannel(Channels.AllTeamsRallyRoom);
        var message = $"Attention all guilders\nDoing a current rank check-in for those seeing this notification.\nBe sure to include what tourney level you are playing in - along with other group details.\n... Who else do you see?\n... How much time is left?";
        var msg = await channel.SendMessageAsync(message);
    }
}