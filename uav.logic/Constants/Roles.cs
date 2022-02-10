using System.Collections.ObjectModel;

namespace uav.logic.Constants;

public static class Roles
{
    public static readonly ulong GuildAccessRole = 876377705501827082ul;
    public static readonly ulong PermanentGuildAccessRole = 938235691337412649ul;
    public static readonly ReadOnlyCollection<ulong> GuildTeams = new ReadOnlyCollection<ulong>(new[] {
        900814071853613068ul, // team 1
        902712361838841936ul, // team 2
        902712424493383740ul, // team 3
        922866455366758441ul, // team 4
        926553184657346660ul, // team 5
    });
    public static readonly ulong Moderator = 525329208096981002ul;
    public static readonly ulong MinerMod = 650772364052660258ul;
    public static readonly ulong HelperMod = 781882277651021834ul;
    //public static readonly ulong CommunityMentor = 883421342760845344ul;
    public static readonly ulong GuildHelper = 903684895421915156ul;
}