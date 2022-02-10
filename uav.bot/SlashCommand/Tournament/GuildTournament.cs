using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand.Tournament;

public class GuildTournament : BaseTournamentSlashCommand
{
    public override SlashCommandBuilder CommandBuilder => new SlashCommandBuilder()
        .WithDescription("Guild Tournament management")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("cleanup")
            .WithDescription("Admin cleanup on aisle 4!")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("really", ApplicationCommandOptionType.Boolean, "Are you really sure?", required: true)
            .AddOption("no-really", ApplicationCommandOptionType.Boolean, "NOnono, I mean REALLY sure?", required: true)
        )
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("contestants")
            .WithDescription("Show the current contestant list.")
            .WithType(ApplicationCommandOptionType.SubCommand)
        )
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("select-teams")
            .WithDescription("Select the teams.")
            .WithType(ApplicationCommandOptionType.SubCommand)
        )
        // .AddOption(new SlashCommandOptionBuilder()
        //     .WithName("join")
        //     .WithDescription("Joins the next guild tournament if it is open")            
        //     .WithType(ApplicationCommandOptionType.SubCommand)
        // ).AddOption(new SlashCommandOptionBuilder()
        //     .WithName("leave")
        //     .WithDescription("Leaves the upcoming guild tournament if it's still open")
        //     .WithType(ApplicationCommandOptionType.SubCommand)
        // ).AddOption(new SlashCommandOptionBuilder()
        //     .WithName("add")
        //     .WithDescription("Admin add user to tournament group")
        //     .WithType(ApplicationCommandOptionType.SubCommand)
        //     .AddOption("user-to-add", ApplicationCommandOptionType.User, "User to add")
        //     .AddOption(new SlashCommandOptionBuilder()
        //         .WithName("team-number")
        //         .WithDescription("Team number to add user to")
        //         .AddChoice("Team 1", 1)
        //         .AddChoice("Team 2", 2)
        //         .AddChoice("Team 3", 3)
        //         .WithType(ApplicationCommandOptionType.Integer)
        //     )
        // ).AddOption(new SlashCommandOptionBuilder()
        //     .WithName("remove")
        //     .WithDescription("Admin remove user from tournament")
        //     .WithType(ApplicationCommandOptionType.SubCommand)
        //     .AddOption("user-to-remove", ApplicationCommandOptionType.User, "User to remove")
        // ).AddOption(new SlashCommandOptionBuilder()
        //     .WithName("winner")
        //     .WithDescription("Admin set winner team")
        //     .WithType(ApplicationCommandOptionType.SubCommand)
        //     .AddOption("team-number", ApplicationCommandOptionType.Integer, "Team that won")
        // )
        ;

    public override Task Invoke(SocketSlashCommand command)
    {
        Func<SocketSlashCommand, Task> subCommand = command.Data.Options.First().Name switch {
            "join" => Join,
            "leave" => Leave,
            "add" => Add,
            "remove" => Remove,
            "winner" => Winner,
            "cleanup" => Cleanup,
            "select-teams" => SelectTeams,
            "contestants" => Contestants,
            _ => throw new NotImplementedException(),
        };
        return subCommand(command);
    }

    private async Task SelectTeams(SocketSlashCommand command)
    {
        var allowed = IsInARole(command.User, Roles.HelperMod, Roles.MinerMod, Roles.Moderator);
        if (!allowed)
        {
            await RespondAsync("You do not have permissions to do this.", ephemeral: true);
            return;
        }

        var service = new Services.Tournament((command.User as SocketGuildUser).Guild);
        

        _ = Task.Run(async () => service.SelectTeams());

        await RespondAsync("Teams are being selected.", ephemeral: true);
    }

    private async Task RemoveRoleFromEveryone(SocketRole role)
    {
        foreach (var user in role.Members)
        {
            await user.RemoveRoleAsync(role.Id);
        }
    }


    private async Task Cleanup(SocketSlashCommand command)
    {
        var allowed = IsInARole(command.User, Roles.HelperMod, Roles.MinerMod, Roles.Moderator);
        if (!allowed)
        {
            await RespondAsync("You do not have permissions to do this.", ephemeral: true);
            return;
        }

        var options = CommandArguments(command.Data.Options.First().Options);
        var really = (bool)options["really"].Value;
        var noReally = (bool)options["no-really"].Value;

        if (!really || !noReally)
        {
            await RespondAsync("I didn't think so.", ephemeral: true);
            return;
        }

        var _guild = (command.User as IGuildUser)?.Guild as SocketGuild;
        _ = Task.Run(ActualCleanup);
        await RespondAsync("Cleaning up roles in the background.", ephemeral: true);

        async Task ActualCleanup()
        {
            // start with clearing the old roles
            await RemoveRoleFromEveryone(_guild.GetRole(Roles.GuildAccessRole));
            foreach (var teamRole in Roles.GuildTeams.Select(_guild.GetRole))
            {
                await RemoveRoleFromEveryone(teamRole);
            }

            // try removing all of the extra emotes.
            try {
                var msg = await _guild.GetTextChannel(905379297219473418ul).GetMessageAsync(900805354408009788ul);
                Emote.TryParse("<:tbdcapitalplanet:852848079699050527>", out var tbdcapitalplanet);
                var userGroups  = msg.GetReactionUsersAsync(tbdcapitalplanet, int.MaxValue);
                await foreach (var users in userGroups)
                {
                    foreach (var user in users)
                    {
                        if (!user.IsBot)
                        {
                            await msg.RemoveReactionAsync(tbdcapitalplanet, user);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType().Name).Warn("Failed to remove reactions:", e);
            }
            finally {}
        }
    }

    private async Task Contestants(SocketSlashCommand command)
    {
        var _guild = (command.User as IGuildUser)?.Guild as SocketGuild;
        var _tournament = new Services.Tournament(_guild);

        var users = _tournament.TournamentContestants().ToArray();
        var thisUserIsIn = users.Any(x => x.user.Id == command.User.Id);
        var allUsers = users.GroupBy(c => c.permanent).ToDictionary(g => g.Key, g => g.OrderBy(u => u.user.Nickname ?? u.user.Username));
        var embedFields = new[] { true, false }.Where(allUsers.ContainsKey)
            .Select(x =>
                new EmbedFieldBuilder().WithName($"{(x ? "Permanent":"This week's")} Guild Members")
                    .WithValue(string.Join("\n", allUsers[x].Select(u => u.user.Mention)))
            ).ToArray();

        var msg = new EmbedBuilder()
            .WithTitle("Current guild contestants")
            .WithDescription($"The following players are registered. {(thisUserIsIn ? "**This includes you**" : "__You are not yet registered__")}")
            .WithFields(embedFields)
            .WithColor(Color.Blue);
        
        await RespondAsync(embed: msg.Build(), ephemeral: true);
    }

    private async Task Join(SocketSlashCommand command)
    {

    }

    private async Task Leave(SocketSlashCommand command)
    {
        
    }

    private async Task Add(SocketSlashCommand command)
    {
        
    }

    private async Task Remove(SocketSlashCommand command)
    {
        
    }

    private async Task Winner(SocketSlashCommand command)
    {

    }
}