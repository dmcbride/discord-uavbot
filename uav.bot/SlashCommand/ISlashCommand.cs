using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand
{
    public interface ISlashCommand
    {
        string CommandName { get; }

        SlashCommandBuilder CommandBuilder { get; }

        Task Invoke(SocketSlashCommand command);
    }
}