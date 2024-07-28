using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Extensions.Attributes;
using Discord.WebSocket;

namespace Discord.Extensions.Interaction;

public abstract class BaseInteractionHandler
{
  public BaseInteractionHandler(SocketInteraction interaction)
  {
    Interaction = interaction;
  }

  protected SocketInteraction Interaction { get; set; }

  protected virtual async Task RespondAsync(string message, bool ephemeral = false)
  {
    await Interaction.RespondAsync(message, ephemeral: ephemeral);
  }
}

// public interface IInteractionHandler2
// {
//     string CommandName { get; }

//     Task<SlashCommandBuilder> CommandBuilderAsync { get; }

//     SocketSlashCommand Command {set;}

//     Task DoCommand();

//     SocketModal Modal {set;}

//     SocketMessageComponent Component {set;}

//     Task DoModal(string[] command);

//     Task DoComponent(ReadOnlyMemory<string> command);
// }

// public interface ICommandHandler<T> where T : ModalHandlerAttribute
// {
//     private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> _commandHandlers = new();
    
//     MethodInfo? GetCommandHandler(string commandName)
//     {
//         var type = GetType();
//         if (!_commandHandlers.ContainsKey(type))
//         {
//             _commandHandlers[type] = new Dictionary<string, MethodInfo>();
//             foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
//             {
//                 var attributes = method.GetCustomAttributes(typeof(T), false);
//                 if (attributes.Length == 0) continue;
//                 var attribute = (T)attributes[0];
//                 _commandHandlers[type][attribute.CommandName] = method;
//             }
//         }

//         return _commandHandlers[type].TryGetValue(commandName, out var handler) ? handler : null;
//     }

//     Task RunCommand(ReadOnlyMemory<string> command)
//     {
//         var method = GetCommandHandler(command.Span[0]);
//         if (method == null) return Task.CompletedTask;
//         return (Task)method.Invoke(this, new object[] {command.Slice(1)})!;
//     }
// }