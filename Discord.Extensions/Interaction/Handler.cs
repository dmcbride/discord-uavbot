using System.Reflection;
using Discord.Extensions.Attributes;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord.Extensions.Interaction;

public class Handler
{
  private readonly DiscordSocketClient _client;
  private readonly Dictionary<string, Type> _slashCommands;
  private readonly Dictionary<string, Type> _buttonCommands;
  private readonly Dictionary<string, Type> _selectMenuCommands;
  private readonly Dictionary<string, Type> _modalCommands;

  public Handler(DiscordSocketClient client, Assembly source)
  {
    _client = client;

    var types = source
        .GetTypes()
        .Where(t => typeof(BaseInteractionHandler).IsAssignableFrom(t) && !t.IsAbstract)
        .ToArray()!;

    _slashCommands = BuildCommands<SlashCommandAttribute, BaseSlashCommandHandler>();
    _buttonCommands = BuildCommands<ButtonCommandAttribute, BaseButtonHandler>();
    _selectMenuCommands = BuildCommands<SelectMenuHandlerAttribute, BaseSelectMenuHandler>();
    _modalCommands = BuildCommands<ModalHandlerAttribute, BaseModalSubmittedHandler>();

    Setup(source);

    Dictionary<string, Type> BuildCommands<TInteractionAttributeType, TBaseType>() where TInteractionAttributeType : BaseInteractionHandlerAttribute
    {
      return types
          .Where(t =>
            typeof(TBaseType).IsAssignableFrom(t) &&
            t.GetCustomAttribute<TInteractionAttributeType>() != null
          )
          .ToDictionary(t => t.GetCustomAttribute<TInteractionAttributeType>()!.Name, t => t);
    }
  }

  private void Setup(Assembly source)
  {
    _client.Ready += OnClientReady;
    if (_slashCommands.Any())
    {
      _client.SlashCommandExecuted += SlashCommandHandler;
    }
    if (_buttonCommands.Any())
    {
      _client.ButtonExecuted += ButtonHandler;
    }
    if (_selectMenuCommands.Any())
    {
      _client.SelectMenuExecuted += SelectMenuHandler;
    }
    if (_modalCommands.Any())
    {
      _client.ModalSubmitted += ModalHandler;
    }
  }

  private async Task ModalHandler(SocketModal modal)
  {
    if (_modalCommands.TryGetValue(modal.Data.CustomId, out var type))
    {
      var obj = (BaseModalSubmittedHandler)Activator.CreateInstance(type, new object[] { modal })!;
      try
      {
        await obj.DoModal();
      }
      catch (Exception e)
      {
        Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }
  }

  private async Task SelectMenuHandler(SocketMessageComponent component)
  {
    if (_selectMenuCommands.TryGetValue(component.Data.CustomId, out var type))
    {
      var obj = (BaseSelectMenuHandler)Activator.CreateInstance(type, new object[] { component })!;
      try
      {
        await obj.DoSelectMenu();
      }
      catch (Exception e)
      {
        Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }
  }

  private async Task ButtonHandler(SocketMessageComponent component)
  {
    if (_buttonCommands.TryGetValue(component.Data.CustomId, out var type))
    {
      var obj = (BaseButtonHandler)Activator.CreateInstance(type, new object[] { component })!;
      try
      {
        await obj.DoButton();
      }
      catch (Exception e)
      {
        Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }
  }

  private async Task SlashCommandHandler(SocketSlashCommand command)
  {
    if (_slashCommands.TryGetValue(command.Data.Name, out var type))
    {
      var obj = (BaseSlashCommandHandler)Activator.CreateInstance(type, new object[] { command })!;
      try
      {
        await obj.DoCommand();
      }
      catch (Exception e)
      {
        Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }
  }

  private Task OnClientReady()
  {
    _ = Task.Run(InternalClientReady);
    return Task.CompletedTask;

    async Task InternalClientReady()
    {
      try
      {
        var commands = new List<SlashCommandProperties>();
        foreach (var (name, type) in _slashCommands)
        {
          var obj = (BaseSlashCommandHandler)Activator.CreateInstance(type)!;
          commands.Add((await obj.CommandBuilderAsync)
              .WithName(name)
              .WithDefaultPermission(true)
              .Build());
        }

        foreach (var guild in _client.Guilds)
        {
          try
          {
            var x = await _client.Rest.BulkOverwriteGuildCommands(commands.ToArray(), guild.Id).ConfigureAwait(false);
          }
          catch (HttpException exception)
          {
            var json = JsonConvert.SerializeObject(exception, Formatting.Indented);
            Console.WriteLine(json);
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }
  }
}