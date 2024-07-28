using Discord.Extensions.Attributes;
using Discord.Extensions.Interaction;
using Discord.WebSocket;

namespace Stock.Bot.Interactions;

[SlashCommand("stock", "Play StockBot!")]
public class Stock : BaseSlashCommandHandler
{
  public Stock(SocketSlashCommand interaction) : base(interaction)
  {
  }

  [SubCommand("hello", "Say hello!")]
  private async Task Hello([SubCommandParameter("Your name", true)] string name)
  {
    await RespondAsync($"Hello {name}!");
  }
}
