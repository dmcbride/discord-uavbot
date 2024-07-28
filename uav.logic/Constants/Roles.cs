using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uav.logic.Constants;

public static class Roles
{
    public static readonly ulong GuildAccessRole = 876377705501827082ul;
    public static readonly ulong PermanentGuildAccessRole = 938235691337412649ul;

    public static readonly IReadOnlyCollection<ulong> GuildRoles = new ReadOnlyCollection<ulong>([
        GuildAccessRole,
        PermanentGuildAccessRole,
    ]);
    
    public static readonly ReadOnlyCollection<ulong> GuildTeams = new ReadOnlyCollection<ulong>([
        900814071853613068ul, // team 1
        902712361838841936ul, // team 2
        902712424493383740ul, // team 3
        // https://discord.com/channels/523911528328724502/985741103301021756/1214894846662152232 -- removing teams 4 and 5.
        // 922866455366758441ul, // team 4
        // 926553184657346660ul, // team 5
    ]);
    public static readonly ulong Moderator = 525329208096981002ul;
    public static readonly ulong DedicatedToTheGame = 970041634190409738ul;
    public static readonly ulong HelperMod = 650772364052660258ul;
    public static readonly ulong Helper = 781882277651021834ul;
    public static readonly ulong TraineeHelper = 986275678993391676ul;
    public static readonly ulong Developer = 525328955843280903ul;
    
    public static readonly ulong GuildHelper = 903684895421915156ul;
    public static readonly ReadOnlyDictionary<int, ulong> TeamWins = new(new Dictionary<int, ulong> {
        [3] = 923561253966839848ul,
        [10] = 923561320035528735ul,
        [25] = 923276979262865540ul,
    });

    public static readonly ISet<ulong> AllMods = new HashSet<ulong> {
        Moderator,
        DedicatedToTheGame,
        HelperMod,
        Helper,
        TraineeHelper,
    };

    public static readonly ISet<ulong> Admins = new HashSet<ulong> {
        Developer,
        Moderator,
        Helper,
        DedicatedToTheGame,
    };

    public static readonly ulong GameRegistered = 980560129772650548ul;
}