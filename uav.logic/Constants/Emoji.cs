using System.Collections.Generic;
using System.Linq;
using Discord;

namespace uav.logic.Constants
{
    public class IpmEmoji
    {
        public static readonly string ipmdm = "<:ipmdm:628302645265956875>";
        public static readonly string four_leaf_clover = "🍀";
        public static readonly string boostcashwindfall = "<:boostcashwindfall:642974216681029644>";
        public static readonly string warning = "⚠️";
        public static readonly string itemTP = "<:item30teleporterTP:1220146420040863835>";
        public static readonly string itemSR = "<:item32subspaceRelaySR:1223487433975533568>";
        public static readonly string itemAR = "<:item33advancedRobotAR:1238242851536638044>";
        public static readonly string ipmCredits = "<:ipmCredits:530812982004023306>";
        public static readonly string partying_face = "🥳";
        public static readonly string tada = "🎉";
        public static readonly string heart = "❤️";
        public static readonly string ipmgalaxy = "<:ipmgalaxy:642968632049270795>";
        public static readonly string turtle = "🐢";
        public static readonly string rabbit2 = "🐇";
        public static readonly string unicorn = "🦄";
        public static readonly string ipmtourney = "<:ipmtourney:848300513724071946>";
        public static readonly string tbdcapitalplanet = "<:tbdcapitalplanet:852848079699050527>";

        public static Emoji[] NumberEmojis = new[] {
            Emoji.Parse("0️⃣"),
            Emoji.Parse("1️⃣"),
            Emoji.Parse("2️⃣"),
            Emoji.Parse("3️⃣"),
            Emoji.Parse("4️⃣"),
            Emoji.Parse("5️⃣"),
            Emoji.Parse("6️⃣"),
            Emoji.Parse("7️⃣"),
            Emoji.Parse("8️⃣"),
            Emoji.Parse("9️⃣"),
            Emoji.Parse("🔟"),
        };

        public static IEnumerable<Emoji> TeamEmojis => NumberEmojis.Skip(1);
        public static Emoji Team(int i) => NumberEmojis[i];

        public static readonly Emoji X = new Emoji("❌");

    }
}