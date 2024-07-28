using System.Reflection;
using Discord.Extensions.Attributes;
using Discord.Extensions.Extensions;
using Discord.WebSocket;

namespace Discord.Extensions.Interaction;

public abstract class BaseSlashCommandHandler : BaseInteractionHandler
{
  protected delegate Task CommandHandler(SocketSlashCommand interaction, IDictionary<string, MethodInfo> options);

  private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> _commandHandlersCache = new();

  private IReadOnlyDictionary<string, MethodInfo> CommandHandlers => _commandHandlersCache.GetOrAdd(GetType(), BuildCommandHandlers);

  private static Dictionary<string, MethodInfo> BuildCommandHandlers(Type t)
  {
    var handlers = new Dictionary<string, MethodInfo>();
    foreach (var method in t.GetMethods())
    {
      var attribute = method.GetCustomAttribute<SubCommandAttribute>();
      if (attribute == null) continue;

      handlers.Add(attribute.CommandName, method);
    }

    return handlers;
  }

  public BaseSlashCommandHandler(SocketSlashCommand interaction) : base(interaction)
  {
  }

  virtual public Task<SlashCommandBuilder> CommandBuilderAsync
  {
    get
    {
      var builder = new SlashCommandBuilder();

      foreach (var method in GetType().GetMethods())
      {
        var attribute = method.GetCustomAttribute<SubCommandAttribute>();
        if (attribute == null) continue;

        var subCommandBuilder = new SlashCommandOptionBuilder()
            .WithName(attribute.CommandName)
            .WithDescription(attribute.Description)
            .WithType(ApplicationCommandOptionType.SubCommand);

        foreach (var parameter in method.GetParameters())
        {
          var parameterAttribute = parameter.GetCustomAttribute<SubCommandParameterAttribute>();
          if (parameterAttribute == null) continue;

          subCommandBuilder.AddOption(parameterAttribute.ToSlashCommandOptionBuilder(parameter));
        }
      }

      return Task.FromResult(builder);
    }
  }

  protected SocketSlashCommand Command => (SocketSlashCommand)base.Interaction!;

  public async Task DoCommand()
  {
    var subcommand = Command.Data.Options.First().Name;
    var subcommandMethod = CommandHandlers.GetOrDefault(subcommand);

    if (subcommandMethod == null)
    {
      await RespondAsync($"Unknown subcommand {subcommand}", ephemeral: true);
      return;
    }

    
  }

}