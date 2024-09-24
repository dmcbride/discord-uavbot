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
using uav.Schedule;

namespace uav;

class UAV
{
    private DiscordSocketClient _client;
    private uav.bot.Periodic.IpmUpdate _updateChecker;
    private static string? secret => logic.Configuration.Config.GetConfig("discord:secret");

    public UAV()
    {
        SetupLog4Net();
        logic.Configuration.Config.LoadConfigs<Program>("Development");

        var config = new DiscordSocketConfig
        {
            GatewayIntents = (
                GatewayIntents.AllUnprivileged |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
                ) &
                // but we don't use these, so don't listen for them.
                ~(GatewayIntents.GuildInvites | GatewayIntents.GuildScheduledEvents),
            AlwaysDownloadUsers = true,
            UseInteractionSnowflakeDate = false,
            MessageCacheSize = 100,
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;

        _updateChecker = new bot.Periodic.IpmUpdate(_client);
    }

    public async Task Start(string[] args)
    {
        var commandService = new CommandService();
        var handler = new Command.Handler(_client, commandService);
        await handler.InstallCommandsAsync();

        var slashHandler = new bot.SlashCommand.Handler(_client, GetType().Assembly);
        var jobScheduler = new Scheduler();
        jobScheduler.AddJob(new bot.Jobs.Tournament(_client));
        jobScheduler.AddJob(new bot.Jobs.Leaderboards(_client));
        jobScheduler.AddJob(new bot.Jobs.Poll(_client));

        await _client.LoginAsync(TokenType.Bot, secret);
        await _client.StartAsync();

        _updateChecker.Start();

        jobScheduler.Start();

        await Task.Delay(-1);

        _updateChecker.Dispose();
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
        rollingFileAppender.ActivateOptions();

        // also log to console
        var consoleAppender = new ConsoleAppender
        {
            Layout = patternLayout,
        };

        hierarchy.Root.AddAppender(rollingFileAppender);
        hierarchy.Root.AddAppender(consoleAppender);

        hierarchy.Configured = true;
    }

    private Task Log(LogMessage message)
    {
        var logger = LogManager.GetLogger(this.GetType());
        Action<object, Exception>? logMethod = message.Severity switch
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