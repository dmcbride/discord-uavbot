using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;

namespace uav.bot.SlashCommand.Stats;

public class Stats : BaseSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Statistics")
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("starred-roles")
                .WithDescription("List the top holders of starred roles")
                .WithType(ApplicationCommandOptionType.SubCommand)
        )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "starred-roles" => StarredRoles,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);

    }

    public async Task StarredRoles(SocketSlashCommand command,  IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var starredRoles = Guild!.Roles.Where(r => r.Name.Contains("🌟"));

        var roleCountByUser = new Dictionary<ulong, uint>();
        var usersById = new Dictionary<ulong, SocketGuildUser>();
        foreach (var role in starredRoles)
        {
            foreach (var user in role.Members)
            {
                if (!roleCountByUser.ContainsKey(user.Id))
                {
                    roleCountByUser[user.Id] = 1;
                }
                else
                {
                    roleCountByUser[user.Id]++;
                }

                usersById[user.Id] = user;
            }
        }

        var topUsers = TopUsers(roleCountByUser, usersById, 10);

        var lastSeenRank = 0u;
        var msg = string.Join("\n", topUsers.Select(u =>
        {
            var msg = (u.last ? "...\n" : "") +
            u.rank switch {
                < 1 => $"You do not have any starred roles, so you are unranked at this time.",
                _ => $"`{(u.rank == lastSeenRank ? " " : u.rank)} [{u.count}]:` {u.user.DisplayName()}",
            };
             
            lastSeenRank = u.rank;
            return msg;
        }));
        var embed = new EmbedBuilder()
            .WithTitle("Users with the most starred roles")
            .WithDescription($"{msg}\n\n{Support.SupportStatement}")
            .WithColor(Color.Gold);
            
        await RespondAsync(embed:embed.Build());

        IEnumerable<(uint rank, SocketGuildUser user, uint count, bool last)> TopUsers(Dictionary<ulong, uint> userCounts, Dictionary<ulong, SocketGuildUser> users, int maxUsers)
        {
            var topUsers = userCounts.Keys.GroupBy(id => userCounts[id])
                .OrderByDescending(g => g.Key)
                .ToArray();
            
            var found = 0u;
            var userSeen = false;
            foreach (var userGroup in topUsers)
            {
                var rank = found + 1;
                foreach (var u in userGroup)
                {
                    ++found;
                    userSeen = userSeen || u == User.Id;
                    yield return (rank, users[u], userCounts[u], false);
                }

                if (found > maxUsers)
                {
                    break;
                }
            }

            if (!users.ContainsKey(User.Id))
            {
                yield return (0, User, 0, true);
            }

            if (!userSeen)
            {
                found = 0;
                foreach (var userGroup in topUsers)
                {
                    if (userGroup.Contains(User.Id))
                    {
                        yield return (found + 1, User, userGroup.Key, true);
                    }
                    found += (uint)userGroup.Count();
                }
            }
        }
    }
}
