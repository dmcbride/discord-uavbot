using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Gv;

public class Gv : BaseGvSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Convert GV numbers between normal and sci notations")
        .AddOption("gv", ApplicationCommandOptionType.String, "Galaxy Value", true);

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var gvInput = (string)options["gv"].Value;

        if (!GV.TryFromString(gvInput, out var gv, out var error))
        {
            return RespondAsync($"Invalid input. Usage: `!gv gv`{(error != null ? $"\n{error}" : string.Empty)}", ephemeral: true);
        }

        var embed = new EmbedBuilder()
            .WithAuthor(command.User.ToString())
            .WithTitle($"GV of {gvInput}")
            .WithDescription($"= {gv}\n\n{Support.SupportStatement}")
            .WithColor(Color.LightOrange);
        return RespondAsync(embed: embed.Build());
    }
}