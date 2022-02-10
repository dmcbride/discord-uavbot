using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Extensions;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Admin;

public class ApplyUserTierToNickname : BaseAdminSlashCommand
{
    private static string superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";
    private static char turtlePrefix = '^';
    private static char rabbitPrefix = '*';

    private string toSuperscriptNumbers(long n)
    {
        var s = new char[n == 0 ? 1 : (int)Math.Log10(n) + 1];
        for (var i = s.Length - 1; i >= 0; i--)
        {
            var m = n % 10;
            n /= 10;

            s[i] = superscripts[(int)m];
        }

        return new string(s);
    }

    private static (string Name, string Pet)[] Pets = {
        ("None", ""),
        ("Rabbit (credit farmer)", "🐇"),
        ("Turtle (long hauler)", "🐢"),
        ("Dragon (tourney/challenge)", "🐲"),
        ("Unicorn (tourney/challenge)", "🦄"),
        ("Worm (tourney only)", "🪱"),
        ("Whale ($$$)", "🐳"),
        ("Robot (UAV Mod)", "🤖"),
        ("Bat (Man)", "🦇"),
        ("Penguin (Arms)", "🐧"),
    };

    private static Regex PetFinder = new Regex($"^((?:{string.Join("|",Pets.Where(p=>p.Pet.Any()).Select(p=>p.Pet))}) ?)?");

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Applies the user's tier to their nickname")
        .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
        .AddOption("best-gv", ApplicationCommandOptionType.String, "GV or tier", isRequired: false)
        .AddOption("sales", ApplicationCommandOptionType.Integer, "Number of sales", isRequired: false)
        .AddOption("pet", ApplicationCommandOptionType.String, "Pet",
            choices: Pets.Select(p => new ApplicationCommandOptionChoiceProperties{Name = p.Name, Value = p.Pet}).ToArray(),
            isRequired: false);

    protected override async Task InvokeAdminCommand(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var user = options["user"].Value as IGuildUser;
        var startingName = user.Nickname ?? user.Username;
        var gvString = (string)options.GetOrDefault("best-gv", null)?.Value;
        var sales = (long?)options.GetOrDefault("sales", null)?.Value;
        var pet = (string)options.GetOrDefault("pet", null)?.Value;
        if (pet?.Length > 0)
        {
            pet += " ";
        }
        
        List<string> suffixes = new();

        if (sales != null)
        {
            suffixes.Add($"{rabbitPrefix}{toSuperscriptNumbers(sales.Value)}");
        }

        if (gvString != null)
        {
            if (!GV.TryFromString(gvString, out var gv, out var errorMessage))
            {
                await RespondAsync($"Invalid GV: {errorMessage}", ephemeral: true);
                return;
            }

            var tier = gv < 1_000 ? (int)gv : gv.Exponential;
            suffixes.Add($"{turtlePrefix}{toSuperscriptNumbers(tier)}");
        }

        if (!suffixes.Any())
        {
            await RespondAsync($"Neither best-gv nor sales given?", ephemeral: true);
            return;
        }

        var nick = user.Username;
        var suffix = " " + string.Join("", suffixes);

        if (!user.Nickname.IsNullOrEmpty())
        {
            nick = Regex.Replace(user.Nickname, $" ?[{superscripts}{turtlePrefix}{rabbitPrefix}]+(?= *[^{superscripts}{turtlePrefix}{rabbitPrefix}]*?$)", suffix);
        }
        else
        {
            nick += suffix;
        }
        
        if (!pet.IsNullOrEmpty())
        {
            nick = PetFinder.Replace(nick, pet);
        }

        if (nick.Length > 32)
        {
            await RespondAsync($"Sorry - that would be too long for a nickname.", ephemeral: true);
            return;
        }

        await (user as SocketGuildUser).ModifyAsync(x => x.Nickname = nick);
        await RespondAsync($"Set nickname to {nick}, was {startingName}", ephemeral: true);
    }
}