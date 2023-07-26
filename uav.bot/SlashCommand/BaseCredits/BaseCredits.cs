
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Database;
using uav.logic.Models;
using uav.logic.Service;

namespace uav.bot.SlashCommand.BaseCredits;

public class BaseCredits : BaseSlashCommand
{
    private readonly Credits creditService = new Credits();
    private readonly Ark arkService = new Ark();

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Interact with the base-credits database")
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("submit")
                .WithDescription("Submit a known GV-to-base-credit pair")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("gv", ApplicationCommandOptionType.String, "GV", isRequired: true)
                .AddOption("base-credits", ApplicationCommandOptionType.Integer, "Base credits", minValue: 10, isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("credits-for")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithDescription("Calculates / estimates the credits for a particular GV")
                .AddOption("gv", ApplicationCommandOptionType.String, "GV", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("gv-for")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithDescription("Estimate the GV required for a given base credits")
                .AddOption("credits", ApplicationCommandOptionType.Integer, "credits", minValue: 10, isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("submit-screenshot")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithDescription("Submit a screenshot of a GV-to-base-credit pair")
                .AddOption("screenshot1", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: true)
                .AddOption("screenshot2", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot3", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot4", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot5", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot6", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot7", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
                .AddOption("screenshot8", ApplicationCommandOptionType.Attachment, "Screenshot", isRequired: false)
        )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);

        if (options.TryGetValue("gv", out var gvInput))
        {
            if (!GV.TryFromString((string)gvInput.Value, out var gv, out var error))
            {
                return RespondAsync($"Invalid GV.{(error != null ? $"\n{error}" : string.Empty)}");
            }

            if (gv < 10_000_000 || gv > 1e109)
            {
                return RespondAsync($"Invalid GV. GV must be between 10M and 1E+109", ephemeral: true);
            }
        }

        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
            "submit" => Submit,
            "credits-for" => CreditsFor,
            "gv-for" => GvFor,
            "submit-screenshot" => SubmitScreenshot,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);
    }

    private async Task SubmitScreenshot(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> dictionary)
    {
        var shots = Enumerable.Range(1, 8)
            .Select(i => dictionary.TryGetValue($"screenshot{i}", out var shot) ? (IAttachment)shot.Value : null)
            .Where(shot => shot != null)
            .ToArray();
        
        await DeferAsync();

        var extractor = new ExtractGvBaseCredits();
        var results = new StringBuilder();
        foreach (var (shot, idx) in shots.Select((shot, idx) => (shot, idx + 1)))
        {
            var extractedValues = await extractor.Extract(shot!.Url);
            if (extractedValues == null)
            {
                results.AppendLine($"Unable to extract values from screenshot {idx}");
                continue;
            }

            var (message, success) = await arkService.UpdateCredits(extractedValues.gv, extractedValues.credits, command.User.ToDbUser());
            if (success)
            {
                var contributionCount = await databaseService.CountByUser(command.User.ToDbUser());
                if (contributionCount.total == 1)
                {
                    message += $" Thank you for your very first contribution!";
                }
                else
                {
                    message += $" You have now contributed **{contributionCount.total}** data point(s), **{contributionCount.distinctBaseCredits}** different base credits, across **{contributionCount.distinctTiers}** tiers.";
                }
                results.AppendLine(message);
            }
            else
            {
                results.AppendLine($"Unable to update credits for screenshot {idx}: {message}");
            }
        }

        await RespondAsync(results.ToString());
    }

    private async Task Submit(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var gv = (string)options["gv"].Value;
        var baseCredits = (int)(long)options["base-credits"].Value;

        var (message, success) = await arkService.UpdateCredits(gv, baseCredits, command.User.ToDbUser());

        if (success)
        {
            var contributionCount = await databaseService.CountByUser(command.User.ToDbUser());
            if (contributionCount.total == 1)
            {
                message += $" Thank you for your very first contribution!";
            }
            else
            {
                message += $" You have now contributed **{contributionCount.total}** data point(s), **{contributionCount.distinctBaseCredits}** different base credits, across **{contributionCount.distinctTiers}** tiers.";
            }
        }

        await RespondAsync(message);
    }

    private async Task CreditsFor(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        // see if we have any user config.
        var userConfig = await databaseService.GetUserConfig(command.User.ToDbUser());

        // by this point, we've already validated it once, so we're good.
        GV.TryFromString((string)options["gv"].Value, out var gv, out var _);
        
        var msg = await arkService.QueryCreditRange(gv!, userConfig);

        await RespondAsync(msg);
        return;
    }

    private async Task GvFor(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var baseCredits = (int)(long)options["credits"].Value;
        var gv = await arkService.GVRequiredForCredits(baseCredits);

        if (gv == GV.Zero)
        {
            await RespondAsync($"There is not yet enough information to guess the GV for {baseCredits}");
            return;
        }

        var msg = $"By my calculations, it appears you can get {baseCredits} {IpmEmoji.ipmCredits} with a GV of approximately **${gv}**.";

        await RespondAsync(msg);
    }
}
