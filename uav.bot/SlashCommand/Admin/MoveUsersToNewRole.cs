using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand.Admin;

public class MoveUsersToNewRole : BaseAdminSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Moves users from one role to another")
        .AddOption("from-role", ApplicationCommandOptionType.Role, "Role to move users from", isRequired: true)
        .AddOption("to-role", ApplicationCommandOptionType.Role, "Role to move users into", isRequired: true);

    protected override async Task InvokeAdminCommand(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var fromRole = (IRole)options["from-role"].Value;
        var toRole = (IRole)options["to-role"].Value;

        await RespondAsync("Moving users - this may take a while");

#pragma warning disable 4014
        Task.Run(async () => {
            var role = (command.User as IGuildUser)?.Guild.GetRole(fromRole.Id) as SocketRole; // Guild Access Role
            var users = role?.Members.ToArray() ?? Enumerable.Empty<IGuildUser>();

            foreach (var user in users)
            {
                await user.AddRoleAsync(toRole.Id);
                await user.RemoveRoleAsync(fromRole.Id);
            }
        });
#pragma warning restore 4014
    }
}