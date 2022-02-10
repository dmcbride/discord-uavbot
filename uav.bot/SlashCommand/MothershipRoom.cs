using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Extensions;

namespace uav.bot.SlashCommand;

public class MothershipRoom : BaseSlashCommand
{
    private static Dictionary<string, int[]> rooms;
    private static string[] roomNames;
    static MothershipRoom() {
        var data = new[] {
            (s: "Engineering", a: new[] {5, 8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 58791, 72313, 102226, 125592, 154215, 189264, 232163, 355816, 436062, 534178, 654095, 800612}), // 49
            (s: "Aeronautical", a: new[] {5, 8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 58791, 72313, 102226, 125592, 154215, 189264, 232163, 355816, 436062, 534178, 654095, 800612}), // 49
            (s: "Packaging", a: new[] {5, 8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 58791, 72313, 102226, 125592, 154215, 189264, 232163, 355816, 436062, 534178, 654095, 800612}), // 49
            (s: "Forge", a: new[] {5, 8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 58791, 72313, 124449, 152894, 187740, 230408, 282633, 554454, 679499, 832389, 1019251, 1247593}), // 49
            (s: "Workshop", a: new[] {5, 8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 58791, 72313, 124449, 152894, 187740, 230408, 282633, 554454, 679499, 832389, 1019251, 1247593}), // 49
            (s: "Astronomy", a: new[] {26, 46, 74, 111, 160, 224, 307, 414, 522, 729}), // 10
            (s: "Laboratory", a: new[] {26, 46, 74, 111, 160, 224, 307, 414, 522, 729}), // 10
            (s: "Terrarium", a: new[] {18, 25, 34, 46, 62, 82, 107, 139, 180, 231}), // 10
            (s: "Lounge", a: new[] {8, 12, 18, 25, 34, 46, 62, 82, 107, 139, 180, 231, 296, 377, 479, 607, 767, 966, 1215, 1524, 1908, 2385, 2976, 3709, 4616, 5737, 7121, 8830, 10938, 13536, 16736, 20673, 25517, 31471, 38786, 47768, 70549, 86776, 106670, 131052, 160920, 197492, 242257, 297029, 436822, 535106, 655234, 802006, 981276}), // 49
            (s: "Robotics", a: new[] {53, 75, 103, 139, 185, 244, 319, 415, 537, 690}), // 10
            (s: "Backup Generator", a: new[] {21, 29, 39, 52, 68, 89, 116, 150, 193, 247, 314, 399, 506, 639, 805, 1012, 1270, 1590, 1987, 2480, 3091, 3846, 4781, 5934, 7359, 9115, 11280, 13946, 17228, 21264, 64226, 32321, 39806, 48992, 60261, 74077, 91008, 111750, 137148, 168234, 206270, 252790, 309668}), // 43
            (s: "Underforge", a: new[] {18, 25, 34, 46, 62, 82, 107, 139, 180, 231}), // 10
            (s: "Dorms", a: new[] {21, 29, 39, 52, 68, 89, 116, 150, 193, 247}), // 10
            (s: "Sales", a: new[] {30, 40, 53, 68, 88, 114, 145, 185, 235, 298, 377, 474, 596, 748, 937, 1171, 1462, 1822, 2267, 2817, 3497, 4337, 5372, 6648, 8219, 10153, 12532, 15456, 19049, 23460, 28874, 35515, 43657, 53636, 65860, 80828, 99149, 121565, 148982, 209878, 256995, 314562, 384875, 470732, 661866, 808948, 988387, 1207246, 1474110}), // 49
            (s: "Classroom", a: new[] {97, 120, 149, 184, 228, 282, 348, 430, 530, 653, 804, 990, 1218, 1497, 1839, 2258, 2771, 3400, 4168, 5108, 6258, 7663, 9379, 11475, 14035, 17160, 26218, 32032, 39125, 59718, 72898, 88958, 108530, 132370, 193687, 236113, 287762, 350628, 427130, 702278, 855126, 1041026, 1267075, 1541907, 2907775, 3537134, 4301921, 5231136, 6359960}), // 49
            (s: "Marketing", a: new[] {69, 86, 107, 134, 167, 208, 259, 321, 398, 493, 610, 754, 932, 1150, 1418, 1748, 2153, 2650, 3259, 4007, 4922, 6044, 7418, 9099, 11157, 13673, 16749, 20509, 25109, 30715, 37567, 45930, 56137, 68589, 83777, 102296, 124872, 152386, 185911, 306114, 373262, 455023, 554558, 675709, 1275862, 1553884, 1892082, 2303405, 2803573}), // 49
            (s: "Planet Relations", a: new[] {139, 171, 210, 258, 317, 388, 476, 583, 715, 875, 1071, 1311, 1603, 1960, 2396, 2927, 3575, 4365, 5329, 6503, 7934, 9676, 11799, 14383, 17530, 21359, 26020, 31690, 38587, 46975, 57176, 69577, 84652, 102974, 125239, 152290, 185153, 225069, 273546, 448754, 545235, 662360, 804524, 977061, 1838968, 2232725, 2710423, 3289895, 3992736}), // 49
            (s: "Belt Studies", a: new[] {147, 181, 222, 272, 334, 410, 503, 616, 755, 924, 1131, 1384, 1693, 2070, 2530, 3092, 3776, 4611, 5628, 6869, 8380, 10220, 12462, 15192, 18515, 22560, 27483, 33471, 40756, 49616, 60390, 73489, 89411, 108763, 132280, 160852, 195562, 237722, 288924, 473982, 575888, 699597, 849755, 1031991, 1942358, 2358249, 2862804, 3474852, 4217208}), // 49
        };
        rooms = data.ToDictionary(d => d.s, d => d.a);
        roomNames = data.Select(d => d.s).ToArray();
    }

    public override SlashCommandBuilder CommandBuilder{
        get {
            var roomOption = new SlashCommandOptionBuilder()
                .WithName("room")
                .WithDescription("Room type")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);

            foreach (var room in roomNames)
            {
                roomOption = roomOption.AddChoice(room, room);
            }

            var cb = new SlashCommandBuilder()
                .WithDescription("Give room costs")
                .AddOption(roomOption)
                .AddOption("level", ApplicationCommandOptionType.Integer, "Desired level", isRequired: true, minValue: 2, maxValue: 50)
                .AddOption("starting-at", ApplicationCommandOptionType.Integer, "Starting level for total calculation", isRequired: false, minValue: 1, maxValue: 49);
            
            return cb;
        }
    }

    public override async Task Invoke(SocketSlashCommand command)
    {
        var options = CommandArguments(command);
        var room = (string)options["room"].Value;
        var level = (int)(long)options["level"].Value;
        var startingAt = (int?)(long?)options.GetOrDefault("starting-at", null)?.Value ?? 1;

        var credits = rooms[room];

        if (level - 2 > credits.Length || level < 2)
        {
            await RespondAsync($"{level} is outside the range of 2 - {credits.Length + 2} for {room}", ephemeral: true);
            return;
        }

        var msg = $"Level {level} costs **{credits[level - 2]:N0} {IpmEmoji.ipmCredits}**\n\nTotal to get here from level {startingAt} is **{credits.Take(level - 1).Skip(startingAt - 1).Sum():N0}** {IpmEmoji.ipmCredits}\n\n{Support.SupportStatement}";
        var embed = EmbedBuilder($"Mothership room cost: {room}", msg, Color.Blue);

        var ephemeral = command.Channel.Id switch {
            Channels.VacuumInSpaceAndTime => false,
            Channels.CreditFarmersAnonymous => false,
            Channels.LongHaulersGang => false,
            _ => true,
        };

        await RespondAsync(embed: embed.Build(), ephemeral: ephemeral);
    }
}
