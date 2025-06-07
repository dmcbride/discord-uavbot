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
  private readonly IReadOnlyList<int> MaxCredits = [5, 4, 4, 4, 6, 8, 1, 10, 5, 5, 6, 6, 2];

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
              .WithDescription("Tell UAV about your current galaxy set up. Only specify the values you want to change.")
              .WithType(ApplicationCommandOptionType.SubCommand)
              .AddOption("lounge-level", ApplicationCommandOptionType.Integer, "Lounge level", minValue: 0, maxValue: 60)
              .AddOption("has-exodus", ApplicationCommandOptionType.Boolean, "Has exodus")
              .AddOption("credits-1", ApplicationCommandOptionType.Integer, "Credits 1", minValue: 0, maxValue: MaxCredits[0])
              .AddOption("credits-2", ApplicationCommandOptionType.Integer, "Credits 2", minValue: 0, maxValue: MaxCredits[1])
              .AddOption("credits-3", ApplicationCommandOptionType.Integer, "Credits 3", minValue: 0, maxValue: MaxCredits[2])
              .AddOption("credits-4", ApplicationCommandOptionType.Integer, "Credits 4", minValue: 0, maxValue: MaxCredits[3])
              .AddOption("credits-5", ApplicationCommandOptionType.Integer, "Credits 5", minValue: 0, maxValue: MaxCredits[4])
              .AddOption("credits-6", ApplicationCommandOptionType.Integer, "Credits 6", minValue: 0, maxValue: MaxCredits[5])
              .AddOption("credits-7", ApplicationCommandOptionType.Integer, "Credits 7", minValue: 0, maxValue: MaxCredits[6])
              .AddOption("credits-8", ApplicationCommandOptionType.Integer, "Credits 8", minValue: 0, maxValue: MaxCredits[7])
              .AddOption("credits-9", ApplicationCommandOptionType.Integer, "Credits 9", minValue: 0, maxValue: MaxCredits[8])
              .AddOption("credits-10", ApplicationCommandOptionType.Integer, "Credits 10", minValue: 0, maxValue: MaxCredits[9])
              .AddOption("credits-11", ApplicationCommandOptionType.Integer, "Credits 11", minValue: 0, maxValue: MaxCredits[10])
              .AddOption("credits-12", ApplicationCommandOptionType.Integer, "Credits 12", minValue: 0, maxValue: MaxCredits[11])
              .AddOption("credits-13", ApplicationCommandOptionType.Integer, "Credits 13", minValue: 0, maxValue: MaxCredits[12])
      )
      .AddOption(
          new SlashCommandOptionBuilder()
              .WithName("get-credit-config")
              .WithDescription("Get your current galaxy set up")
              .WithType(ApplicationCommandOptionType.SubCommand)
      )
      ;

  public override Task Invoke(SocketSlashCommand command)
  {
    var options = CommandArguments(command.Data.Options.First().Options);

    Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch
    {
      "player-id" => PlayerId,
      "get-player-id" => GetPlayerId,
      "set-credit-config" => SetCreditConfig,
      "get-credit-config" => GetCreditConfig,
      _ => throw new NotImplementedException(),
    };
    return subCommand(command, options);
  }

  private async Task GetCreditConfig(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> dictionary)
  {
    var config = await databaseService.GetUserConfig(User.ToDbUser());
    var multipliers = config.GetMultipliers();

    await RespondAsync($@"Your current config is:
  Lounge level: {config.LoungeLevel}
  Exodus? {(config.HasExodus ? "Yes :partying_face:" : "No :sob:")}
  Credits 1: {config.Credits1}/{MaxCredits[0]}
  Credits 2: {config.Credits2}/{MaxCredits[1]}
  Credits 3: {config.Credits3}/{MaxCredits[2]}
  Credits 4: {config.Credits4}/{MaxCredits[3]}
  Credits 5: {config.Credits5}/{MaxCredits[4]}
  Credits 6: {config.Credits6}/{MaxCredits[5]}
  Credits 7: {config.Credits7}/{MaxCredits[6]}
  Credits 8: {config.Credits8}/{MaxCredits[7]}
  Credits 9: {config.Credits9}/{MaxCredits[8]}
  Credits 10: {config.Credits10}/{MaxCredits[9]}
  Credits 11: {config.Credits11}/{MaxCredits[10]}
  Credits 12: {config.Credits12}/{MaxCredits[11]}
  Credits 13: {config.Credits13}/{MaxCredits[12]}
  Lounge multiplier: x{multipliers.LoungeMultiplier:N2}
  Station multiplier: x{multipliers.StationMultiplier:N2}
  Exodus multiplier: x{multipliers.ExodusMultiplier}
  ", ephemeral: true);
  }

  private async Task SetCreditConfig(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> dictionary)
  {
    // load the user's current config
    var config = await databaseService.GetUserConfig(User.ToDbUser());
    logger.Debug($"Current config: {config}");

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
    if (dictionary.ContainsKey("credits-9"))
    {
      config.Credits9 = (int)(long)dictionary["credits-9"].Value;
    }
    if (dictionary.ContainsKey("credits-10"))
    {
      config.Credits10 = (int)(long)dictionary["credits-10"].Value;
    }
    if (dictionary.ContainsKey("credits-11"))
    {
      config.Credits11 = (int)(long)dictionary["credits-11"].Value;
    }
    if (dictionary.ContainsKey("credits-12"))
    {
      config.Credits12 = (int)(long)dictionary["credits-12"].Value;
    }
    if (dictionary.ContainsKey("credits-13"))
    {
      config.Credits13 = (int)(long)dictionary["credits-13"].Value;
    }

    logger.Debug($"New config: {config}");

    await databaseService.UpdateUserConfig(config);

    logger.Debug("Updated config");

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
    var id = (await databaseService.GetUserPlayerIds([command.User.Id])).ToArray();

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