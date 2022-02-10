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
}