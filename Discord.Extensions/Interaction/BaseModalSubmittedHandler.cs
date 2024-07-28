using Discord.WebSocket;

namespace Discord.Extensions.Interaction;

public abstract class BaseModalSubmittedHandler : BaseInteractionHandler
{
  public BaseModalSubmittedHandler(SocketInteraction interaction) : base(interaction)
  {
  }

  public abstract Task DoModal();
}
