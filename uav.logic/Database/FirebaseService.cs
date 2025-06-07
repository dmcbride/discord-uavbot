namespace uav.logic.Database;

using System.Collections.Generic;
using Firebase.Database;
using Firebase.Database.Query;

public class FirebaseService
{
    private record Player(string name, double level);
    private record TournamentData(Dictionary<string, Player> Players, int count);

    private static FirebaseClient? _client => Configuration.Config.GetConfig("firebase:url") is string connStr
        ? new FirebaseClient(connStr)
        : null;
    

}