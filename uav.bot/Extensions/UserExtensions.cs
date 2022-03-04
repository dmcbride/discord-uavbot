using Discord;
using Discord.WebSocket;
using uav.logic.Database.Model;

public static class UserExtensions

{
    private class DbUser : IDbUser
    {
        SocketGuildUser _u;
        public DbUser(SocketGuildUser u)
        {
            _u = u;
        }
        public ulong User_Id => _u.Id;

        public string User_Name => _u.Username;

        public string User_Nick => _u.Nickname;
    }

    public static IDbUser ToDbUser(this SocketGuildUser u) => new DbUser(u);

    // let's face it, everything we're doing actually has SocketGuildUser under the covers
    // so make it easier.
    public static IDbUser ToDbUser(this SocketUser u) => new DbUser(u as SocketGuildUser);

    public static string DisplayName(this SocketGuildUser u) => u.Nickname ?? u.Username;
}