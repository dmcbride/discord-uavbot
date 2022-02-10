
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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
                .AddOption("gv", ApplicationCommandOptionType.String, "GV", required: true)
                .AddOption("base-credits", ApplicationCommandOptionType.Integer, "Base credits", minValue: 10, required: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("credits-for")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithDescription("Calculates / estimates the credits for a particular GV")
                .AddOption("gv", ApplicationCommandOptionType.String, "GV", required: true)
        )
        // .AddOption(
        //     new SlashCommandOptionBuilder()
        //         .WithName("gv-for")
        //         .WithType(ApplicationCommandOptionType.SubCommand)
        //         .WithDescription("Estimate the GV required for a given base credits")
        //         .AddOption("credits", ApplicationCommandOptionType.Integer, "credits", minValue: 10, required: true)
        // )
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
            _ => throw new NotImplementedException(),
        };
        return subCommand(command, options);
    }

    private async Task Submit(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var gv = (string)options["gv"].Value;
        var baseCredits = (int)(long)options["base-credits"].Value;

        var (message, success) = await arkService.UpdateCredits(gv, baseCredits, command.User.ToString());

        if (success)
        {
            var contributionCount = await databaseService.CountByUser(command.User.ToString());
            if (contributionCount.total == 1)
            {
                message += $"Thank you for your very first contribution!";
            }
            else
            {
                message += $"You have now contributed **{contributionCount.total}** data point(s), **{contributionCount.distinctBaseCredits}** different base credits, across **{contributionCount.distinctTiers}** tiers.";
            }
        }

        await RespondAsync(message);
    }

    private async Task CreditsFor(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        // by this point, we've already validated it once, so we're good.
        GV.TryFromString((string)options["gv"].Value, out var gv, out var _);
        
        var msg = await arkService.QueryCreditRange(gv);

        await RespondAsync(msg);
        return;
    }

    private Task GvFor(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var baseCredits = (long)options["base-credits"].Value;
        return Task.CompletedTask; // fixme
    }
}