using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;
using uav.logic.Service;
using Discord.Extensions.Extensions;
using log4net.Core;
using log4net;

namespace uav.bot.SlashCommand.Admin;

public class Admin : BaseAdminSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Admin tools")
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("player-id")
                .WithDescription("Retrieve player IDs")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user1", ApplicationCommandOptionType.User, "User", isRequired: true)
                .AddOption("user2", ApplicationCommandOptionType.User, "User", isRequired: false)
                .AddOption("user3", ApplicationCommandOptionType.User, "User", isRequired: false)
                .AddOption("user4", ApplicationCommandOptionType.User, "User", isRequired: false)
                .AddOption("user5", ApplicationCommandOptionType.User, "User", isRequired: false)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("user-from-player-id")
                .WithDescription("Retrieve user from player ID")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("player-id", ApplicationCommandOptionType.String, "Player ID", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("giveaway")
                .WithDescription("W00t!")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("winners", ApplicationCommandOptionType.Integer, "How many", isRequired: true, minValue: 1, maxValue: 25)
                .AddOption("min-message-count", ApplicationCommandOptionType.Integer, "Minimum messages (default 400)", isRequired: false, minValue: 100)
                .AddOption("include-mods", ApplicationCommandOptionType.Boolean, "Include mods as potential winners", isRequired: false)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("register-another-user")
                .WithDescription("Register someone else's player ID in case their client is crapping out")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
                .AddOption("settings", ApplicationCommandOptionType.Attachment, "Settings screen shot", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("test")
                .WithDescription("Testing for Jefferson")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("d", ApplicationCommandOptionType.String, "Data", isRequired: true)
                .AddOption("s", ApplicationCommandOptionType.Integer, "Sig", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("remove-user-hint")
                .WithDescription("Remove a user's hint")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
                .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
        )
        ;

    protected override IEnumerable<ulong> AllowedRoles => Roles.Admins;

    protected override Task InvokeAdminCommand(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "player-id" => PlayerId,
            "giveaway" => Giveaway,
            "register-another-user" => RegisterOther,
            "remove-user-hint" => RemoveUserHint,
            "test" => Render,
            "user-from-player-id" => UserFromPlayerId,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);
    }

    private async Task UserFromPlayerId(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> dictionary)
    {
        var playerId = (string)dictionary["player-id"].Value;
        logger.Info($"UserFromPlayerId: {playerId}");
        await DeferAsync(ephemeral: true);
        try
        {
            var user = await databaseService.GetUserFromPlayerId(playerId);
            logger.Info($"UserFromPlayerId: {user?.User_Id}");
            if (user == null)
            {
                logger.Info($"UserFromPlayerId: No user found for player ID {playerId}");
                await RespondAsync($"No user found for player ID {playerId}", ephemeral: true);
                return;
            }
            logger.Info($"UserFromPlayerId: User found for player ID {playerId}: {user.User_Id}");
            await RespondAsync($"User found for player ID {playerId}: <@{user.User_Id}>", ephemeral: true);
        }
        catch (Exception e)
        {
            logger.Error($"UserFromPlayerId: {e.Message}");
            await RespondAsync($"Error: {e.Message}", ephemeral: true);
        }
    }

    private async Task PlayerId(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var users = Enumerable.Range(1,5).Select(i => $"user{i}").Select(u => options!.GetOrDefault(u, null)?.Value)
            .Cast<SocketGuildUser>()
            .Where(x => x != null);
        
        var players = (await databaseService.GetUserPlayerIds(users.Select(u => u.Id)))
            .ToDictionary(x => x.User_Id, x => x.Player_Id.IfNullOrEmptyThen("Unknown"));
        
        var output = string.Join("\n", users.Select(u => $"User {u.Mention}: {players.GetOrDefault(u.Id, "Unknown User")}"));

        await RespondAsync(output);
    }

    private async Task Giveaway(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var winners = (long)options["winners"].Value;
        var minMsg = options.GetOrDefault("min-message-count", null)?.Value as long? ?? 400;
        var includeMods = options.GetOrDefault("include-mods", null)?.Value as bool? ?? false;
        var lastMonth = DateTime.UtcNow.AddMonths(-1);

        var winningUsers = await databaseService.GetTopParticipationHistoryWinners((int)minMsg, (int)winners, lastMonth.Year, lastMonth.Month, includeMods);
        var winningUserOutput = winningUsers.Select((u, i) => $"{i+1}: {MentionUtils.MentionUser(u.User_Id)}{(u.Player_Id.HasValue() ? $" ({u.Player_Id})" : string.Empty)}");

        var output = string.Join("\n", winningUserOutput);
        var embed = new EmbedBuilder()
            .WithTitle("This month's winners")
            .WithDescription(output)
            .WithColor(Color.Green);
 
        await RespondAsync(embed: embed.Build(), ephemeral: false);
    }

    private async Task RegisterOther(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var user = (IUser)options["user"].Value;
        var settings = (IAttachment)options["settings"].Value;

        await DeferAsync(ephemeral: true);

        var extractor = new ExtractPlayerId();
        var id = await extractor.Extract(settings.Url);

        if (id != null)
        {
            await databaseService.RegisterUser(user.Id, id);
            _ = (user as SocketGuildUser)?.AddRoleAsync(Roles.GameRegistered);
            await RespondAsync($"Registered {user.Mention}'s player ID as {id}", ephemeral: true);
            return;
        }
        await RespondAsync($"Can't figure out a player ID for {user.Mention}, please contact Tanktalus", ephemeral: true);
    }

    private readonly byte[] key = new byte[] {
        15, 23, 97, 156, 51
    };

    private async Task Render(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var data = (string)options["d"].Value;
        var sig = (long)options["s"].Value;

        var byteData = Convert.FromBase64String(data);
        var actualData = Encoding.UTF8.GetString(byteData);

        var hash = GetHashCode(byteData.Concat(key));

        if (hash != sig)
        {
            await RespondAsync($"Invalid input.", ephemeral: true);
            return;
        }

        await RespondAsync($"Received {actualData}");
    }

    private int GetHashCode(IEnumerable<byte> data)
    {
        unchecked
        {
            int hash = 17;
            foreach(var d in data)
            {
                hash = hash * 23 + d.GetHashCode();
            }
            return hash;
        }
    }

    private async Task RemoveUserHint(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var user = (SocketUser)options["user"].Value;
        var shortcut = (string)options["shortcut"].Value;
        var hint = await databaseService.GetHint(user.ToDbUser(), shortcut);
        if (hint == null)
        {
            await RespondAsync($"No hint found for {user.Mention} with shortcut {shortcut}", ephemeral: true);
            return;
        }
        await databaseService.RemoveHint(hint);
        await RespondAsync($"Deleted hint for {user.Mention} with shortcut {shortcut}", ephemeral: true);
    }
}