using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public class Hint : BaseSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Used by Tanktalus for testing. You're not allowed.")
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("add-hint")
                .WithDescription("Add or update one of your hints")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("remove-hint")
                .WithDescription("Remove one of your hints")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("users")
                .WithDescription("List all users who have provided hints")
                .WithType(ApplicationCommandOptionType.SubCommand)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("hints-from-user")
                .WithDescription("List all hints from a user")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("list-hints")
                .WithDescription("List all hints")
                .WithType(ApplicationCommandOptionType.SubCommand)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("get-hint")
                .WithDescription("Get hint")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("user", ApplicationCommandOptionType.User, "User (defaults to yourself)", isRequired: false)
                .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
        )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "add-hint" => AddHint,
            "remove-hint" => RemoveHint,
            "users" => Users,
            "list-hints" => ListHints,
            "hints-from-user" => HintsFromUser,
            "get-hint" => GetHint,
            _ => throw new NotImplementedException(),
        };

        return subCommand(command, options);
    }

    private static CacheItemPolicy CacheItemPolicy => new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)};
    private static MemoryCache _modalCache = MemoryCache.Default;

    private async Task AddHint(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var shortcut = (string)options["shortcut"].Value;

        var hint = await databaseService.GetHint(Command.User.ToDbUser(), shortcut);
        var shortcutId = Guid.NewGuid().ToString();
        _modalCache.Add(shortcutId, shortcut, CacheItemPolicy);

        var mb = ModalBuilder("add-hint", shortcutId)
            .WithTitle(hint == null ? $"New Hint: {shortcut}" : $"Update {shortcut} Hint")
            .AddTextInput("Title", "title", value: hint?.Title)
            .AddTextInput("Hint Text", "hint-text", TextInputStyle.Paragraph, value: hint?.HintText)
            ;
        
        await RespondAsync(mb);
    }

    private async Task RemoveHint(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var shortcut = (string)options["shortcut"].Value;
        var hint = await databaseService.GetHint(Command.User.ToDbUser(), shortcut);
        if (hint == null)
        {
            await RespondAsync($"No hint found for {shortcut}");
            return;
        }
        await databaseService.RemoveHint(hint);
        await RespondAsync($"Hint removed for {shortcut}", ephemeral: true);
    }

    private async Task Users(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var users = await databaseService.GetHintUsers();
        var embed = new EmbedBuilder()
            .WithTitle("Users with Hints")
            .WithDescription(string.Join("\n", users.Select(u => $"{u.Name()}")))
            .Build();
        await RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task HintsFromUser(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var user = (SocketUser)options["user"].Value;
        var hints = await databaseService.GetHints(user.ToDbUser());
        var dbUser = await databaseService.GetUser(user.Id);
        var embed = new EmbedBuilder()
            .WithTitle($"Hints for {dbUser.Name()}")
            .WithDescription(string.Join("\n", hints.Select(h => $"{h.HintName} ({h.Title})")))
            .Build();
        await RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task ListHints(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var hints = (await databaseService.GetAllHints()).OrderBy(h => h.HintName);
        var users = (await databaseService.GetHintUsers()).ToDictionary(u => u.User_Id, u => u.Name());
        var embed = new EmbedBuilder()
            .WithTitle("Hints")
            .WithDescription(string.Join("\n", hints.Select(h => $"`{h.HintName}` ({h.Title}) by `{users[h.UserId]}`")))
            .Build();
        await RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task GetHint(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var user = (SocketUser)options.GetOrDefault("user")?.Value;
        var shortcut = (string)options["shortcut"].Value;
        var hint = await databaseService.GetHint(user?.ToDbUser(), shortcut);
        if (hint == null)
        {
            await RespondAsync($"No hint found from {user?.Mention ?? "anyone"} called {shortcut}", ephemeral: true);
            return;
        }
        user ??= Guild.GetUser(hint.UserId);
        var embed = new EmbedBuilder()
            .WithTitle(hint.Title)
            .WithDescription($"{hint.HintText}\n\nby {user.Mention}")
            .Build();
        
        // for now, only show registered-users' hints. Probably should use a different role for this.
        var ephemeral = !Guild.GetUser(user.Id).Roles.Select(r => r.Id).Any(id => id == Roles.GameRegistered);
        await RespondAsync(embed: embed, ephemeral: ephemeral);
    }

    protected override Task InvokeModal(string[] command)
    {
        var c = command[0];
        command = command.Skip(1).ToArray();
        switch (c)
        {
            case "add-hint":
                return ModalAddHint(command);
            default:
                return base.InvokeModal(command);
        }
    }

    private async Task ModalAddHint(string[] command)
    {
        var shortcutId = command[0];
        var shortcut = (string)_modalCache.Get(shortcutId);
        if (shortcut == null)
        {
            await RespondAsync("That just took too long, I forgot what you were talking about. (I may have been rebooted.)", ephemeral: true);
            return;
        }

        var options = Modal.Data.Components.ToDictionary(c => c.CustomId, c => c);
        var title = options["title"].Value;
        var text = options["hint-text"].Value;

        var hint = new uav.logic.Database.Model.Hint(User.ToDbUser(), shortcut, title, text);
        await databaseService.AddHint(hint);
        await RespondAsync("Hint added.", ephemeral: true);
        _modalCache.Remove(shortcutId);

        var updateMsg = $"{User.Mention} added a hint for `{shortcut}`:\n\n----\n{text}\n----\n\nIf this is not okay, you can remove it with `/admin remove-user-hint`";
        _ = Guild.GetTextChannel(Channels.BotTesterConf).SendMessageAsync(updateMsg);
    }
}
