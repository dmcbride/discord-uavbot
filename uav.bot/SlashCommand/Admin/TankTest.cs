// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Discord;
// using Discord.WebSocket;
// using uav.logic.Constants;

// namespace uav.bot.SlashCommand.Admin;

// public class TankTest : BaseAdminSlashCommand
// {
//     protected override IEnumerable<ulong> AllowedRoles => new[] {Roles.MinerMod};
    
//     public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
//         .WithDescription("Used by Tanktalus for testing. You're not allowed.");

//     protected override async Task InvokeAdminCommand(SocketSlashCommand command)
//     {
//         var fields = new[] {
//             new EmbedFieldBuilder()
//                 .WithIsInline(true)
//                 .WithName("field1")
//                 .WithValue("a\nb\nc\nd"),
//             new EmbedFieldBuilder()
//                 .WithIsInline(true)
//                 .WithName("field2")
//                 .WithValue("e\nf\ng\nh"),
//             new EmbedFieldBuilder()
//                 .WithIsInline(true)
//                 .WithName("field3")
//                 .WithValue("a\nb\nc\nd"),
//             new EmbedFieldBuilder()
//                 .WithIsInline(true)
//                 .WithName("field4")
//                 .WithValue("e\nf\ng\nh"),
//             new EmbedFieldBuilder()
//                 .WithIsInline(false)
//                 .WithName("\u200B")
//                 .WithValue(Support.SupportStatement),
//         };
//         var notificationEmbed = new EmbedBuilder()
//             .WithTitle("title")
//             .WithDescription("hello")
//             .WithColor(Color.Blue)
//             .WithFields(fields)
//             //.WithFooter(Support.SupportStatement)
//             .Build();
//         await RespondAsync(embed: notificationEmbed, ephemeral: true);
//     }
// }
