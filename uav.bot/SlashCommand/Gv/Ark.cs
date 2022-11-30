using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Gv;

public class Ark : BaseGvSlashCommand
{
    public Ark()
    {
    }

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Given your current GV, target GV, and cash-on-hand, how many cash arks it will take to reach.")
        .AddOptions(new[] {
            new SlashCommandOptionBuilder()
                .WithName("current-gv")
                .WithDescription("Current total Galaxy Value (GV)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String),
            new SlashCommandOptionBuilder()
                .WithName("target-gv")
                .WithDescription("Target GV")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String),
            new SlashCommandOptionBuilder()
                .WithName("coh")
                .WithDescription("Current cash-on-hand (defaults to current GV)")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.String),
            new SlashCommandOptionBuilder()
                .WithName("hours-per-day")
                .WithDescription("Hour per day of active arking")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Integer)
                .WithMinValue(0)
                .WithMaxValue(24)
        });

    public override async Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var gv = (string)options["current-gv"].Value;
        var goalGv = (string)options["target-gv"].Value;
        var cash = (string)options.GetOrDefault("coh", null)?.Value ?? gv;
        var hoursPerDay = (long?)options.GetOrDefault("hours-per-day", null)?.Value;

        if (!GV.TryFromString(goalGv, out var goalGvValue, out var error) ||
            !GvCash.TryFromStrings(gv, cash, out var initial, out error))
        {
            await RespondAsync($"Invalid input.  Usage: `/ark currentGV goalGV cashOnHand`{(error != null ? $"\n{error}" : string.Empty)}", ephemeral: true);
            return;
        }

        if (goalGvValue < initial.gv)
        {
            await RespondAsync($"Your goal is already reached. Perhaps you meant to reverse them?", ephemeral: true);
            return;
        }

        var final = initial.ArkUntilGv(goalGvValue, out var totalArks, out var arksBeforeCrossover);

        // here we're assuming that you get about 7 cash arks per hour (6 minutes per ark, 10 arks per hour, 70% cash)
        var minHours = Math.Floor(totalArks / (cashArkChance * arksPerHour));
        var maxHours = Math.Ceiling(totalArks / (cashArkChance * arksPerHour));
        var hours = minHours == maxHours 
            ? $"{minHours} hour{(minHours == 1 ? string.Empty:"s")}"
            : minHours == 0
            ? $"1 hour or less"
            : $"{minHours} - {maxHours} hours";
        var daysMessage = string.Empty;
        if (hoursPerDay > 0)
        {
            var minDays = Math.Floor(minHours / hoursPerDay.Value);
            var maxDays = Math.Ceiling(maxHours / hoursPerDay.Value);
            var days = minDays == maxDays
                ? $"{minDays} day{(minDays == 1 ? string.Empty:"s")}"
                : minDays == 0
                ? $"1 day or less"
                : $"{minDays} - {maxDays} days";
            daysMessage = $"\nAt about {hoursPerDay} hours of arking per day, that is about {days}";
        }

        // and then if we got that many arks in that time, we should get about 30/70 of that in DM.
        var dm = Math.Floor(totalArks * dmArkChance / cashArkChance);

        var earlyGameHint = arksBeforeCrossover < 7 || initial.gv > GV.FromString("1Q")
            ? ""
            : $"\nAnother option to improve GV is to focus on ores and getting to the end-game items, such as {IpmEmoji.itemTP} and {IpmEmoji.itemFR} first.";
        var crossoverMessage = arksBeforeCrossover switch
        {
            0 when initial.CashPercent() > 0.75 => "",
            0 => "\nYour cash-on-hand is already above the ark crossover point. Keep it up!",
            _ when arksBeforeCrossover == totalArks =>
                final.CountArksUntilCrossover() switch {
                    0 => $"\nUpon reaching your goal, your cash-on-hand will also barely exceed the ark crossover point. Lucky guess!{earlyGameHint}",
                    var arks => $"\nUpon reaching your goal, you would still need an additional {ZeroOrMoreCash(arks, "ark")} until your cash-on-hand reaches the ark crossover point.{earlyGameHint}",
                },
            _ => $"\nAfter {ZeroOrMoreCash(arksBeforeCrossover, "ark")} towards the goal, your cash-on-hand will reach the ark crossover point.{earlyGameHint}",
        };

        var message = $@"To get to a GV of **{goalGvValue}** from **{initial.gv}** starting with cash-on-hand of **{initial.cash}**, you need **{totalArks}** {IpmEmoji.boostcashwindfall} arks bringing you to a GV of **{final.gv}**.
At about {arksPerHour * cashArkChance} {IpmEmoji.boostcashwindfall} arks per hour, that is about {hours}.{daysMessage}
During this time, you can expect to get about {dm} {IpmEmoji.ipmdm} arks, for a total of {5 * dm} {IpmEmoji.ipmdm}.{crossoverMessage}

{Support.SupportStatement}";
        var embed = new EmbedBuilder()
            .WithDescription(message);
        await RespondAsync(embed: embed.Build());
    }
}