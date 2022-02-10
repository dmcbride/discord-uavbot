// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Discord;
// using Discord.WebSocket;
// using uav.logic.Constants;
// using uav.logic.Extensions;

// namespace uav.bot.SlashCommand.Admin;

// public class SetNick : BaseAdminSlashCommand
// {
//     protected override IEnumerable<ulong> AllowedRoles => new[] {Roles.MinerMod};

//     public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
//         .WithDescription("Just for Tanktalus' use, you're not allowed.")
//         .AddOption("user", ApplicationCommandOptionType.User, "User", isRequired: true)
//         .AddOption("nick", ApplicationCommandOptionType.String, "Nick", isRequired: false);

//     protected override async Task InvokeAdminCommand(SocketSlashCommand command)
//     {
//         var options = CommandArguments(command);
//         var user = options["user"].Value as IGuildUser;
//         var nick = (string)options.GetOrDefault("nick", null)?.Value;

//         await (user as SocketGuildUser).ModifyAsync(x => x.Nickname = nick);
//         await RespondAsync("Done.", ephemeral: true);
//     }
// }