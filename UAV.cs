using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace uav
{
    class UAV
    {
        private DiscordSocketClient _client;
        private readonly static string secret = Environment.GetEnvironmentVariable("uav_secret");

        public UAV()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
        }

        public async Task Start(string[] args)
        {
            var commandService = new CommandService();
            var handler = new Command.Handler(_client, commandService);
            await handler.InstallCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, secret);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.ForegroundColor = message.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.DarkGray,
                LogSeverity.Debug => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}