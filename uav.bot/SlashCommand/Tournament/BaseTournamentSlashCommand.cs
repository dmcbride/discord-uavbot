using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand.Tournament;

public abstract class BaseTournamentSlashCommand : BaseSlashCommand
{
    protected string SpanToReadable(TimeSpan span) => uav.logic.Models.Tournament.SpanToReadable(span);
}