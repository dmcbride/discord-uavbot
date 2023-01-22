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
        var cash = (string?)options.GetOrDefault("coh", null)?.Value ?? gv;
        var hoursPerDay = (long?)options.GetOrDefault("hours-per-day", null)?.Value;

        if (!GV.TryFromString(gv, out var gvValue, out var error) ||
            !GV.TryFromString(goalGv, out var goalGvValue, out error) ||
            !GV.TryFromString(cash, out var cashValue, out error))
        {
            await RespondAsync($"Invalid input.  Usage: `!ark currentGV goalGV cashOnHand`{(error != null ? $"\n{error}" : string.Empty)}", ephemeral: true);
            return;
        }

        if (goalGvValue < gvValue)
        {
            await RespondAsync($"Your goal is already reached. Perhaps you meant to reverse them?", ephemeral: true);
            return;
        }

        if (cashValue > gvValue)
        {
            await RespondAsync($"Your cash on hand ({cash}) is more than your current GV ({gv}), that's probably wrong.", ephemeral: true);
            return;
        }

        if (cashValue < gvValue * 0.54)
        {
            await RespondAsync($"This calculator does not (yet) handle cash-on-hand under 54% of your current GV. You are better off not arking yet anyway. Focus on ores and getting to the end-game items, such as {IpmEmoji.itemTP} and {IpmEmoji.itemFR} first.", ephemeral: true);
            return;
        }

        var (arks, newValue) = ArkCalculate(gvValue, goalGvValue, cashValue, 1.0475d);

        // here we're assuming that you get about 7 cash arks per hour (6 minutes per ark, 10 arks per hour, 70% cash)
        var minHours = Math.Floor(arks / (cashArkChance * arksPerHour));
        var maxHours = Math.Ceiling(arks / (cashArkChance * arksPerHour));
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
            var days = minDays == maxHours
                ? $"{minDays} day{(minDays == 1 ? string.Empty:"s")}"
                : minDays == 0
                ? $"1 day or less"
                : $"{minDays} - {maxDays} days";
            daysMessage = $"\nAt about {hoursPerDay} hours of arking per day, that is about {days}";
        }

        // and then if we got that many arks in that time, we should get about 30/70 of that in DM.
        var dm = Math.Floor(arks * dmArkChance / cashArkChance);


        var message = $@"To get to a GV of **{goalGvValue}** from **{gvValue}** starting with cash-on-hand of **{cashValue}**, you need **{arks}** {IpmEmoji.boostcashwindfall} arks bringing you to a GV of **{newValue}**.
At about {arksPerHour * cashArkChance} {IpmEmoji.boostcashwindfall} arks per hour, that is about {hours}.{daysMessage}
During this time, you can expect to get about {dm} {IpmEmoji.ipmdm} arks, for a total of {5 * dm} {IpmEmoji.ipmdm}.

{Support.SupportStatement}";
        var embed = new EmbedBuilder()
            .WithDescription(message);
        await RespondAsync(embed: embed.Build());
    }
}