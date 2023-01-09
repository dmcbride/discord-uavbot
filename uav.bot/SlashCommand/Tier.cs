using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Models;

namespace uav.bot.SlashCommand;

public class Tier : BaseSlashCommand
{
    public Tier()
    {
        
    }

    public override string CommandName => "tier";

    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Displays GV for a given tier")
        .AddOption("tier-number", ApplicationCommandOptionType.Integer, "Tier Number", true);

    public override async Task Invoke(SocketSlashCommand command)
    {
        var tierNumber = ((long)command.Data.Options.First().Value);
        var gv = GV.FromNumber(Math.Pow(10d, tierNumber + 5));
        await RespondAsync($"Tier {tierNumber} starts at {gv}.");
    }
}