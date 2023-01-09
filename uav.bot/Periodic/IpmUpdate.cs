using System;
using System.Timers;
using Discord.WebSocket;

namespace uav.bot.Periodic;

/// <summary>
/// Check for updated IPM?
/// </summary>
public class IpmUpdate : IDisposable
{
    private const ulong GuildId = 523911528328724502;
    private const ulong VacuumChannelId = 673848862439636998;
    private const ulong ServerNewsId = 823623190172401694;

    private const int everyHours = 6;
    private DiscordSocketClient _client;
    private Timer _timer;
    private SocketTextChannel updateChannel;

    public IpmUpdate(DiscordSocketClient client)
    {
        _client = client;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public void Start()
    {
        _client.Ready += () =>
        {
            _timer = new Timer(everyHours * 60 * 60 * 1000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            updateChannel = _client.GetGuild(GuildId).GetTextChannel(VacuumChannelId);

            return System.Threading.Tasks.Task.CompletedTask;
        };
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        Console.WriteLine("Replace me with actual checking.");
    }
}