using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace uav.bot.SlashCommand;

public class PlanetInfo : BaseSlashCommand
{
    public override Task<SlashCommandBuilder> CommandBuilderAsync
    {
        get {
            return RealGet();
            async Task<SlashCommandBuilder> RealGet()
            {
                var planetNames = (await databaseService.Planets()).ToArray();
                var ores = await databaseService.Ores();

                var c = new SlashCommandBuilder()
                    .WithDescription("Interrogates information about planets")
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("by-name")
                            .WithDescription("Information about each planet")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("planet", ApplicationCommandOptionType.String, "Planet name", isRequired: true),
                        new SlashCommandOptionBuilder()
                            .WithName("by-id")
                            .WithDescription("Information about each planet")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("id", ApplicationCommandOptionType.Integer, "Planet ID", isRequired: true, minValue: 1, maxValue: planetNames.Length),
                        new SlashCommandOptionBuilder()
                            .WithName("by-ore")
                            .WithDescription("Information about the planets producing the given ore")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("ore", ApplicationCommandOptionType.String, "Ore", isRequired: true, choices: ores.Select(o => new ApplicationCommandOptionChoiceProperties{Name=o,Value=o}).ToArray())
                    )
                    ;

                return c;
            }

        }
    }

    public override Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command.Data.Options.First().Options);
        Func<SocketSlashCommand, IDictionary<string, SocketSlashCommandDataOption>, Task> impl = command.Data.Options.First().Name switch {
            "by-name" => ByName,
            "by-id" => ById,
            "by-ore" => ByOre,
            _ => throw new NotImplementedException($"Unknown command {command.Data.Name}."),
        };
        return impl.Invoke(command, options);
    }
    private async Task ByName(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var name = (string)options["planet"].Value;
        var planet = await databaseService.PlanetByName(name);

        if (planet is null)
        {
            await RespondAsync(@$"Unknown planet name ""{name}"".", ephemeral: true);
        }

        await RespondAsync(embed: EmbedForPlanet(planet!));
    }

    private async Task ById(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var id = (int)(long)options["id"].Value;
        var planet = await databaseService.PlanetByNumber(id);

        await RespondAsync(embed: EmbedForPlanet(planet));
    }

    private async Task ByOre(SocketSlashCommand command, IDictionary<string, SocketSlashCommandDataOption> options)
    {
        var ore = (string)options["ore"].Value;
        var planets = await databaseService.PlanetsByOre(ore);

        var embeds = planets.Select(p => EmbedForPlanet(p)).ToArray();

        await RespondAsync(embeds: embeds);
    }

    private Embed EmbedForPlanet(logic.Database.Model.PlanetInfo? planet)
    {
        var ores = new List<string>();

        if (planet!.Ore1 is not null)
        {ores.Add($"{planet.Ore1} ({planet.Ore1Yield}%)");}

        if (planet.Ore2 is not null)
        {ores.Add($"{planet.Ore2} ({planet.Ore2Yield}%)");}

        if (planet.Ore3 is not null)
        {ores.Add($"{planet.Ore3} ({planet.Ore3Yield}%)");}

        var embed = new EmbedBuilder()
            .WithTitle($"Planet Info for {planet.Name} ({planet.Id})")
            .WithFields(
                new EmbedFieldBuilder().WithName("Unlock Cost").WithValue(planet.CostGV),
                new EmbedFieldBuilder().WithName("Ores").WithValue(string.Join("\n", ores))
            )
            .WithColor(Color.DarkGreen)
            ;

        return embed.Build();
    }
}