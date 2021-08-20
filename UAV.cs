using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net.Layout;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Config;

namespace uav
{
    class UAV
    {
        private DiscordSocketClient _client;
        private readonly static string secret = Environment.GetEnvironmentVariable("uav_secret");

        public UAV()
        {
            SetupLog4Net();

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

        private void SetupLog4Net()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders();
            hierarchy.Root.Level = Level.Debug;

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date{yyyy-MM-dd HH:mm:ss.fff} [%logger] %message%newline%exception",
            };
            patternLayout.ActivateOptions();

            var rollingFileAppender = new RollingFileAppender
            {
                AppendToFile = true,
                File = @"Logs/uav.log",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaxFileSize = 1_000_000,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true,
                Encoding = System.Text.Encoding.UTF8,
            };
            rollingFileAppender.ActivateOptions()   ;
            hierarchy.Root.AddAppender(rollingFileAppender);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
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
            
            var logger = LogManager.GetLogger(this.GetType());
            Action<object, Exception> logMethod = message.Severity switch
            {
                LogSeverity.Critical => logger.Fatal,
                LogSeverity.Error => logger.Error,
                LogSeverity.Warning => logger.Warn,
                LogSeverity.Info => logger.Info,
                LogSeverity.Verbose => logger.Info,
                LogSeverity.Debug => logger.Debug,
                _ => null
            };
            logMethod?.Invoke(message.Message, message.Exception);

            return Task.CompletedTask;
        }
    }
}