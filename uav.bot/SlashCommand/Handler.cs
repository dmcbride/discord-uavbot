using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.SlashCommand
{
    public class Handler
    {
        private DiscordSocketClient _client;
        private Dictionary<string, ISlashCommand> _commands;
        
        public Handler(DiscordSocketClient client)
        {
            _client = client;
        }

        private void Setup()
        {
            _client.Ready += OnClientReady;

            // typeof(Handler)
            // .Assembly
            // .
        }

        public async Task OnClientReady()
        {
            foreach (var guild in _client.Guilds)
            {
                var guildId = guild.Id;

                
            }

            
        }
    }
}