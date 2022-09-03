using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;
using uav.logic.Service;

namespace uav.bot.SlashCommand.Stats;

public class Register : BaseSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Register with UAV")
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("player-id")
                .WithDescription("Register your player ID")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("settings", ApplicationCommandOptionType.Attachment, "Settings screen screenshot", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("get-player-id")
                .WithDescription("Check your registered player ID")
                .WithType(ApplicationCommandOptionType.SubCommand)
        )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "player-id" => PlayerId,
            "get-player-id" => GetPlayerId,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);
    }

    protected override bool IsEphemeral(bool? _) => true;

    private async Task PlayerId(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var settings = (IAttachment)options["settings"].Value;

        await DeferAsync(ephemeral: true);

        var extractor = new ExtractPlayerId();
        var id = await extractor.Extract(settings.Url);

        if (id != null)
        {
            await databaseService.RegisterUser(User.ToDbUser(), id);
            _ = User.AddRoleAsync(Roles.GameRegistered);
            await RespondAsync($"Registered your player ID as {id}", ephemeral: true);
            return;
        }
        await RespondAsync($"Can't figure out a player ID, please contact Tanktalus", ephemeral: true);
    }

    private async Task GetPlayerId(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var id = (await databaseService.GetUserPlayerIds(new[] {command.User.Id})).ToArray();

        if (id.Length > 0 && id[0].Player_Id.HasValue())
        {
            await RespondAsync($"Your player ID is {id[0].Player_Id}");
        }
        else
        {
            await RespondAsync("You are not yet registered. Use the `/register player-id` command to do so!");
        }
    }
}