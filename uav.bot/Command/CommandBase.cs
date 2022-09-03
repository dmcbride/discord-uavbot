using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using uav.logic.Database;

namespace uav.Command
{
    public class CommandBase : ModuleBase<SocketCommandContext>
    {
        protected bool IsInARole(params string[] requiredRoles)
        {
            if (!(Context.User is SocketGuildUser user))
            {
                return false;
            }

            var roles = requiredRoles.ToHashSet();
            return user.Roles.Select(r => r.Name).Any(roles.Contains);
        }

        protected bool IsInARole(params ulong[] requiredRoles)
        {
            if (!(Context.User is SocketGuildUser user))
            {
                return false;
            }

            var roles = requiredRoles.ToHashSet();
            return user.Roles.Select(r => r.Id).Any(roles.Contains);
        }

        protected async Task ReplyAsDMAsync(string message)
        {
            await new DatabaseService().SaveUser(Context.User.ToDbUser());
            var channel = await Context.User.CreateDMChannelAsync();
            await channel.SendMessageAsync(message);
        }

        protected EmbedBuilder EmbedBuilder(string title, string message, Color color)
        {
            return new EmbedBuilder()
                .WithAuthor(Context.User.ToString(), Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithTitle(title)
                .WithDescription(message)
                .WithColor(color)
                .WithCurrentTimestamp();
        }

        protected async Task ReplyAsync(EmbedBuilder embed)
        {
            await new DatabaseService().SaveUser(Context.User.ToDbUser());
            await ReplyAsync(embed: embed.Build());
        }
    }
}