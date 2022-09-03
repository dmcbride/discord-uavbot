using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand
{
    public interface ISlashCommand
    {
        string CommandName { get; }

        Task<SlashCommandBuilder> CommandBuilderAsync { get; }

        SocketSlashCommand Command {set;}

        Task DoCommand();

        SocketModal Modal {set;}
        Task DoModal(string[] command);
    }
}