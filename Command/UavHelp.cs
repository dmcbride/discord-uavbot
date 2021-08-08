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
    public class UavHelp : ModuleBase<SocketCommandContext>
    {
        public static readonly string CommandChar = "!";
        private static Dictionary<string, (string Usage, string Summary)> AllCommands = typeof(UavHelp).Assembly
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

        private static (string Summary, string Usage) CommandHelp(MethodInfo m)
        {
            var summary = m.GetCustomAttribute<SummaryAttribute>();
            var usage = m.GetCustomAttribute<UsageAttribute>();
            return (summary?.Text ?? string.Empty, usage?.Usage ?? string.Empty);
        }

        private static string HelpText =
            string.Join("\n", AllCommands.Keys.OrderBy(k => k)
            .SelectMany(key => new[] {
            @$"{CommandChar}{key}: {AllCommands[key].Usage}",
            $"    Usage: `{CommandChar}{key} {AllCommands[key].Summary}`"}));


        [Command("uav")]
        [Summary("List UAV commands")]
        public async Task Help()
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            //channel = await Context.User.GetOrCreateDMChannelAsync() as SocketDMChannel; // not sure, but some reports indicate two of these may be required?
            await channel.SendMessageAsync(HelpText);
        }

    }
}