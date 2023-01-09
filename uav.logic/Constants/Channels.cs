using System.Collections.Generic;

namespace uav.logic.Constants;

public static class Channels
{
    public const ulong DiscordServerNews = 823623190172401694ul;
    public const ulong RoleClaims = 677924152669110292ul;
    public const ulong NewsletterSubmissions = 955376762173399050ul;

    public const ulong IdleMinersHangout = 525330951723548693ul;
    public const ulong AllAboutTodaysGalaxy = 627916074247127108ul;
    public const ulong AsteroidPics = 890147249269645333ul;

    public const ulong VacuumInSpaceAndTime = 673848862439636998ul;

    public const ulong AllTeamsRallyRoom = 899407911900573716ul;
    public const ulong GuildRules = 900801095788527616ul;
    public const ulong WinnersLogbook = 927549912495837204ul;
    public const ulong SuggestionsVenue = 648900470831710238ul;

    public const ulong BugReports = 937745257611812942ul;

    public static readonly ulong[] TeamChannels = new [] {
        900799574413828126ul, // team-1
        900833755932004392ul, // team-2
        902597639093256282ul, // team-3
        923560902270255165ul, // team-4
        939323270501629972ul, // team-5
    };

    public const ulong SubmitFinalRanksHere = 903046199765000202ul;

    public const ulong TournamentTalks = 1000828553581051985ul;
    public const ulong PlatinumTournament = 904738763912126506ul;

    public const ulong CreditFarmersAnonymous = 905379245709201519ul;
    public const ulong LongHaulersGang = 871129095805747310ul;

    public static class Records
    {
        public const ulong SingleGalaxyRecords = 851858349212303370ul;
        public const ulong LegacyGalaxyRecords = 851908260859609158ul;
        public const ulong ChallengeRecords = 996754772856160306ul;
        public const ulong TournamentRecords = 996757651537010749ul;
        public const ulong SpecialtyRecords = 931901051974582304ul;
        public const ulong OverallRankings = 931901499175469126ul;
        public const ulong OverviewAndClaim = 851857881661308978ul;
        public const ulong RecordsForum = 852907559563427850ul;
    }

    public const ulong LoungeOffTopic = 536532203203461122ul;
    public const ulong MessageLog = 887273315528474634ul;

    private static HashSet<ulong> _participationChannels;
    public static ISet<ulong> ParticipationChannels
    {
        get
        {
            if (_participationChannels == null)
            {
                _participationChannels = 
                    new HashSet<ulong>(TeamChannels) {
                        RoleClaims,
                        NewsletterSubmissions,
                        IdleMinersHangout,
                        AllAboutTodaysGalaxy,
                        AsteroidPics,
                        SuggestionsVenue,
                        RoleClaims,
                        TournamentTalks,
                        PlatinumTournament,
                        LongHaulersGang,
                        CreditFarmersAnonymous,
                        AllTeamsRallyRoom,
                        Records.OverviewAndClaim,
                        Records.RecordsForum,
                    };
            }
            return _participationChannels;
        }
    }

    public const ulong BotTesterConf = 888011885570588712ul;
}
