using System.Linq;
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
    }
}