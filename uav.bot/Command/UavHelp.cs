using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using uav.Attributes;

namespace uav.Command
{
    public class UavHelp : CommandBase
    {
        public static readonly string CommandChar = "!";
        private static Dictionary<string, (string Usage, string Summary, RequiredRoleAttribute[] RolesRequired)> AllCommands = typeof(UavHelp).Assembly
            .GetTypes()
            .Where(typeof(ModuleBase<SocketCommandContext>).IsAssignableFrom)
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
            .ToDictionary(CommandName, CommandHelp)
            ;

        private static string CommandName(MethodInfo m)
        {
            var command = m.GetCustomAttribute<CommandAttribute>();
            return command.Text;
        }

        private static (string Summary, string Usage, RequiredRoleAttribute[] RequiredRoles) CommandHelp(MethodInfo m)
        {
            var summary = m.GetCustomAttribute<SummaryAttribute>();
            var usage = m.GetCustomAttribute<UsageAttribute>();
            var rolesRequired = m.GetCustomAttributes<RequiredRoleAttribute>().ToArray();
            return (summary?.Text ?? string.Empty, usage?.Usage ?? string.Empty, rolesRequired);
        }

        private string HelpText =>
            string.Join("\n", AllCommands.OrderBy(kv => kv.Key)
            .Where(kv => kv.Value.RolesRequired.All(r => r.IsAcceptable(Context.User)))
            .SelectMany(kv => new[] {
            @$"{CommandChar}{kv.Key}: {kv.Value.Usage}",
            $"    Usage: `{CommandChar}{kv.Key} {kv.Value.Summary}`"}));


        [Command("uav")]
        [Summary("List UAV commands")]
        public async Task Help()
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(HelpText);
        }

    }
}