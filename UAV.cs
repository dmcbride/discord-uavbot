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
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}