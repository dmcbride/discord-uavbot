using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Extensions.Extensions;
using uav.logic.Extensions;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Admin;

public class ApplyUserTierToNickname : BaseAdminSlashCommand
{
    private static string superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";
    private static char gvTierPrefix = '^';
    private static char salesPrefix = '*';

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
        ("Snake (challenge only)", "🐍"),
        ("Robot (UAV Mod)", "🤖"),
        ("Bat (Man)", "🦇"),
        ("Penguin (Arms)", "🐧"),
        ("Lion (3-win streak in guilds)", "🦁"),
        ("Seal (ark ark! contest winner)", "🦭"),
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

    private static readonly Regex beforeEndOfNickname = new Regex(@$"(?= *[^{superscripts}{gvTierPrefix}{salesPrefix}\sa-zA-Z]*?$)");
    private static readonly string salesLooksLike = @$"[{salesPrefix}][{superscripts}]+(?=[^{superscripts}]|$)";
    private static readonly Regex findSales = new Regex(salesLooksLike);
    private static readonly Regex afterSales = new Regex($"(?<={salesLooksLike})");
    private static readonly string gvTierLooksLike = @$"{gvTierPrefix}[{superscripts}]+(?=[^{superscripts}]|$)";
    private static readonly Regex findGvTier = new Regex(gvTierLooksLike);
    private static readonly Regex beforeGvTier = new Regex($"(?={gvTierLooksLike})");

    protected override async Task InvokeAdminCommand(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var user = (IGuildUser)options["user"].Value;
        var startingName = user.Nickname ?? user.Username;
        var gvString = (string?)options.GetOrDefault("best-gv", null)?.Value;
        var sales = (long?)options.GetOrDefault("sales", null)?.Value;
        var pet = (string?)options.GetOrDefault("pet", null)?.Value;
        
        var nick = startingName;

        if (sales != null)
        {
            var salesIndicator = sales == 0 ? string.Empty : $"{salesPrefix}{toSuperscriptNumbers(sales.Value)}";

            if (findSales.IsMatch(nick))
            {
                nick = findSales.Replace(nick, salesIndicator);
            }
            else if (findGvTier.IsMatch(nick))
            {
                nick = beforeGvTier.Replace(nick, salesIndicator);
            }
            else
            {
                nick = beforeEndOfNickname.Replace(nick, salesIndicator);
            }
        }

        if (gvString != null)
        {
            if (!GV.TryFromString(gvString, out var gv, out var errorMessage))
            {
                await RespondAsync($"Invalid GV: {errorMessage}", ephemeral: true);
                return;
            }

            var tier = gv < 1_000 ? (int)gv : gv.Exponential;

            var tierIndicator = tier == 0 ? string.Empty : $"{gvTierPrefix}{toSuperscriptNumbers(tier)}";
            Console.WriteLine($"tier = {tier}, indicator = {tierIndicator}");
            if (findGvTier.IsMatch(nick))
            {
                Console.WriteLine("Found tier");
                nick = findGvTier.Replace(nick, tierIndicator);
            }
            else if (findSales.IsMatch(nick))
            {
                Console.WriteLine("after sales");
                nick = afterSales.Replace(nick, tierIndicator);
            }
            else
            {
                Console.WriteLine("before end");
                nick = beforeEndOfNickname.Replace(nick, tierIndicator);
            }
        }

        // /apply-user-tier-to-nickname user:@Tommy Salami#5759 best-gv:349q sales:773 pet:Rabbit (credit farmer)
        if (pet != null)
        {
            if (!pet.IsNullOrEmpty())
            {
                pet += " ";
            }
            nick = PetFinder.Replace(nick, pet);
        }

        if (nick.Length > 32)
        {
            await RespondAsync($"Sorry - '{nick}' would be too long for a nickname.", ephemeral: true);
            return;
        }

        await ((SocketGuildUser)user).ModifyAsync(x => x.Nickname = nick);
        await RespondAsync($"Set nickname to `{nick}`, was `{startingName}`", ephemeral: true);
    }
}
