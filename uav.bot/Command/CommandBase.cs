using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace uav.Command
{
    public class CommandBase : ModuleBase<SocketCommandContext>
    {
        protected bool IsInRole(string requiredRole)
        {
            if (!(Context.User is SocketGuildUser user))
            {
                return false;
            }

            var roles = user.Roles;
            return roles.Any(r => r.Name == requiredRole);
        }

        protected async Task ReplyAsDMAsync(string message)
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(message);
        }
    }
}