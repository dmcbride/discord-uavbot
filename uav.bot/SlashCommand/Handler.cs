using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace uav.bot.SlashCommand
{
    public class Handler
    {
        private DiscordSocketClient _client;
        private Dictionary<string, Type> _commands;
        
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
                .ToDictionary(sc => sc.CommandName, sc => sc.GetType());
        }

        public Task OnClientReady()
        {
            Task.Run(InternalClientReady);
            return Task.CompletedTask;

            async Task InternalClientReady() {
                try {
                    var commands = new List<SlashCommandProperties>();
                    foreach (var type in _commands.Values)
                    {
                        var obj = (ISlashCommand)Activator.CreateInstance(type);
                        commands.Add((await obj.CommandBuilderAsync)
                            .WithName(obj.CommandName)
                            .WithDefaultPermission(true)
                            .Build());
                    }
                
                    foreach (var guild in _client.Guilds)
                    {
                        object x = null;
                        try
                        {
                            x = await _client.Rest.BulkOverwriteGuildCommands(commands.ToArray(), guild.Id).ConfigureAwait(false);
                        }
                        catch (HttpException exception)
                        {
                            var json = JsonConvert.SerializeObject(exception, Formatting.Indented);
                            Console.WriteLine(json);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
                }
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (!_commands.TryGetValue(command.Data.Name, out var commandType))
                return;
            var slashCommand = (ISlashCommand)Activator.CreateInstance(commandType);
            slashCommand.Command = command;

            try
            {
                await slashCommand.DoCommand();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to invoke {command.Data.Name}: {e}");
            }
        }
    }
}