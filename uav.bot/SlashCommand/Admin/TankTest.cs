// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Discord;
// using Discord.WebSocket;
// using uav.logic.Constants;

// namespace uav.bot.SlashCommand.Admin;

// public class TankTest : BaseAdminSlashCommand
// {
//     protected override IEnumerable<ulong> AllowedRoles => new[] {Roles.Helper};
    
//     public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
//         .WithDescription("Used by Tanktalus for testing. You're not allowed.")
//         .AddOption(
//             new SlashCommandOptionBuilder()
//                 .WithName("add-hint")
//                 .WithDescription("Add or update one of your hints")
//                 .WithType(ApplicationCommandOptionType.SubCommand)
//                 .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
//         )
//         .AddOption(
//             new SlashCommandOptionBuilder()
//                 .WithName("remove-hint")
//                 .WithDescription("Remove one of your hints")
//                 .WithType(ApplicationCommandOptionType.SubCommand)
//                 .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
//         )
//         .AddOption(
//             new SlashCommandOptionBuilder()
//                 .WithName("users")
//                 .WithDescription("List all users who have provided hints")
//                 .WithType(ApplicationCommandOptionType.SubCommand)
//         )
//         .AddOption(
//             new SlashCommandOptionBuilder()
//                 .WithName("hints-from-user")
//                 .WithDescription("List all hints from a user")
//                 .WithType(ApplicationCommandOptionType.SubCommand)
//                 .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
//         )
//         .AddOption(
//             new SlashCommandOptionBuilder()
//                 .WithName("get-hint")
//                 .WithDescription("Get hint")
//                 .WithType(ApplicationCommandOptionType.SubCommand)
//                 .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
//                 .AddOption("shortcut", ApplicationCommandOptionType.String, "Hint Shortcut", isRequired: true)
//         )
//         ;

//     protected override Task InvokeAdminCommand(SocketSlashCommand command)
//     {
//         var options = CommandArguments(command.Data.Options.First().Options);

//         Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> subCommand = command.Data.Options.First().Name switch {
//             "add-hint" => AddHint,
//             "remove-hint" => RemoveHint,
//             "users" => Users,
//             "hints-from-user" => HintsFromUser,
//             "get-hint" => GetHint,
//             _ => throw new NotImplementedException(),
//         };

//         return subCommand(command, options);
//     }
// }
