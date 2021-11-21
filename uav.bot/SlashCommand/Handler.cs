using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace uav.bot.SlashCommand
{
    public class Handler
    {
        private DiscordSocketClient _client;
        private Dictionary<string, ISlashCommand> _commands;
        
        public Handler(DiscordSocketClient client, Assembly source)
        {
            _client = client;

            Setup(source);
        }

        private void Setup(Assembly source)
        {
            _client.Ready += OnClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;

            _commands = source
                .GetTypes()
                .Where(t => typeof(ISlashCommand).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (ISlashCommand)Activator.CreateInstance(t))
                .ToDictionary(sc => sc.CommandName);
        }

        public async Task OnClientReady()
        {
            var commands = _commands.Values
                .Select(sc => sc.CommandBuilder
                    .WithName(sc.CommandName)
                    .WithDefaultPermission(true)
                    .Build())
                .ToArray();
            
            foreach (var guild in _client.Guilds)
            {
                try
                {
                    await _client.Rest.BulkOverwriteGuildCommands(commands, guild.Id);
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                    Console.WriteLine(json);
                }
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (!_commands.TryGetValue(command.Data.Name, out var action))
                return;
            
            try
            {
                await action.Invoke(command);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to invoke {command.Data.Name}: {e}");
            }
        }
    }
}