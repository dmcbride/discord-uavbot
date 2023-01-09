using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Gv;

public class CashWindfalls : BaseGvSlashCommand
{
    public CashWindfalls()
    {
        
    }

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Given your current GV and target GV, how many cash windfalls it will take to reach.")
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
        });

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var gv = (string)options["current-gv"].Value;
        var goalGv = (string)options["target-gv"].Value;

        if (!GvCash.TryFromStrings(gv, "0", out var initial, out var error) ||
            !GV.TryFromString(goalGv, out var goalGvValue, out error))
        {
            return RespondAsync($"Invalid input. Usage: `/cw currentGV goalGV`{(error != null ? $"\n{error}" : string.Empty)}", ephemeral: true);
        }

        if (goalGvValue < initial.gv)
        {
            return RespondAsync($"Your goal is already reached. Perhaps you meant to reverse them?", ephemeral: true);
        }

        var final = initial.CashWindfallUntilGv(goalGvValue, out var cws);

        var dmRequired = cws * 30;
        var message = @$"To get to a GV of {goalGvValue} from {initial.gv}, you need {ZeroOrMoreCash(cws, "boost")} which will take you to {final.gv}. This may cost up to {dmRequired} {IpmEmoji.ipmdm}

{Support.SupportStatement}";
        var embed = new EmbedBuilder()
            .WithDescription(message);
        return RespondAsync(embed: embed.Build());
    }
}