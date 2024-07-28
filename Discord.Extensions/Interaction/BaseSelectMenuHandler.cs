using Discord.WebSocket;

namespace Discord.Extensions.Interaction;

public abstract class BaseSelectMenuHandler : BaseInteractionHandler
{
  public BaseSelectMenuHandler(SocketInteraction interaction) : base(interaction)
  {
  }

  public abstract Task DoSelectMenu();
}
