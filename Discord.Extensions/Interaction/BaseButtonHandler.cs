using Discord.WebSocket;

namespace Discord.Extensions.Interaction;

public abstract class BaseButtonHandler : BaseInteractionHandler
{
  public BaseButtonHandler(SocketInteraction interaction) : base(interaction)
  {
  }

  protected SocketMessageComponent Component => (SocketMessageComponent)base.Interaction!;

  public abstract Task DoButton();
}
