using Discord.WebSocket;

namespace Discord.Extensions;

public class Bot
{
  private string Token { get; }
  protected DiscordSocketClient Client { get; private set; } = null!;

  public Bot(string token)
  {
    Token = token;
  }

  /// <summary>
  /// Setup anything required before the bot starts.
  /// </summary>
  protected virtual Task PreStartup() => Task.CompletedTask;

  /// <summary>
  /// Setup anything required after the bot starts.
  /// </summary>
  protected virtual Task PostStartup() => Task.CompletedTask;

  /// <summary>
  /// Configure the DiscordSocketClient. Opportunity to set up logging, event handlers, etc.
  /// </summary>
  /// <returns></returns>
  protected virtual Task ConfigureClient() => Task.CompletedTask;

  /// <summary>
  /// The gateway intents to use for the bot.
  /// </summary>
  protected virtual GatewayIntents GatewayIntents => (
    GatewayIntents.AllUnprivileged |
    GatewayIntents.GuildMembers |
    GatewayIntents.GuildMessages |
    GatewayIntents.MessageContent
    ) &
    // but we don't use these, so don't listen for them.
    ~(GatewayIntents.GuildInvites | GatewayIntents.GuildScheduledEvents);
  
  /// <summary>
  /// The configuration to use for the DiscordSocketClient.
  /// </summary>
  protected virtual DiscordSocketConfig DiscordConfig => new DiscordSocketConfig
  {
    GatewayIntents = GatewayIntents,
    AlwaysDownloadUsers = true,
    UseInteractionSnowflakeDate = false,
    MessageCacheSize = 100,
  };

  public async Task Start(string[] args)
  {
    await PreStartup();

    Client = new DiscordSocketClient(DiscordConfig);
    await ConfigureClient();

    await Client.LoginAsync(TokenType.Bot, Token);
    await Client.StartAsync();

    var handler = new Interaction.Handler(Client, GetType().Assembly);

    await PostStartup();

    await Task.Delay(-1);
  }
}