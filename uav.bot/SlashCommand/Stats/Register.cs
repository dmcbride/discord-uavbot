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
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("set-credit-config")
                .WithDescription("Set your current galaxy set up")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("lounge-level", ApplicationCommandOptionType.Integer, "Lounge level", minValue: 0, maxValue: 50)
                .AddOption("has-exodus", ApplicationCommandOptionType.Boolean, "Has exodus")
                .AddOption("credits-1", ApplicationCommandOptionType.Integer, "Credits 1", minValue: 0, maxValue: 5)
                .AddOption("credits-2", ApplicationCommandOptionType.Integer, "Credits 2", minValue: 0, maxValue: 4)
                .AddOption("credits-3", ApplicationCommandOptionType.Integer, "Credits 3", minValue: 0, maxValue: 4)
                .AddOption("credits-4", ApplicationCommandOptionType.Integer, "Credits 4", minValue: 0, maxValue: 4)
                .AddOption("credits-5", ApplicationCommandOptionType.Integer, "Credits 5", minValue: 0, maxValue: 6)
                .AddOption("credits-6", ApplicationCommandOptionType.Integer, "Credits 6", minValue: 0, maxValue: 8)
                .AddOption("credits-7", ApplicationCommandOptionType.Integer, "Credits 7", minValue: 0, maxValue: 1)
                .AddOption("credits-8", ApplicationCommandOptionType.Integer, "Credits 8", minValue: 0, maxValue: 10)
        )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "player-id" => PlayerId,
            "get-player-id" => GetPlayerId,
            "set-credit-config" => SetCreditConfig,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);
    }

private async Task SetCreditConfig(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> dictionary)
{
    // load the user's current config
    var config = await databaseService.GetUserConfig(User.ToDbUser());

    // update the config with the new values
    if (dictionary.ContainsKey("lounge-level"))
    {
        config.LoungeLevel = (int)(long)dictionary["lounge-level"].Value;
    }
    if (dictionary.ContainsKey("has-exodus"))
    {
        config.HasExodus = (bool)dictionary["has-exodus"].Value;
    }
    if (dictionary.ContainsKey("credits-1"))
    {
        config.Credits1 = (int)(long)dictionary["credits-1"].Value;
    }
    if (dictionary.ContainsKey("credits-2"))
    {
        config.Credits2 = (int)(long)dictionary["credits-2"].Value;
    }
    if (dictionary.ContainsKey("credits-3"))
    {
        config.Credits3 = (int)(long)dictionary["credits-3"].Value;
    }
    if (dictionary.ContainsKey("credits-4"))
    {
        config.Credits4 = (int)(long)dictionary["credits-4"].Value;
    }
    if (dictionary.ContainsKey("credits-5"))
    {
        config.Credits5 = (int)(long)dictionary["credits-5"].Value;
    }
    if (dictionary.ContainsKey("credits-6"))
    {
        config.Credits6 = (int)(long)dictionary["credits-6"].Value;
    }
    if (dictionary.ContainsKey("credits-7"))
    {
        config.Credits7 = (int)(long)dictionary["credits-7"].Value;
    }
    if (dictionary.ContainsKey("credits-8"))
    {
        config.Credits8 = (int)(long)dictionary["credits-8"].Value;
    }

    await databaseService.UpdateUserConfig(config);

    await RespondAsync($"Updated your config.", ephemeral: true);
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